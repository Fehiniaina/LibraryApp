using Library.Application.Authors.Commands.CreateAuthor;
using Library.Domain.Interfaces;
using Library.Domain.Interfaces.Services;
using Library.Infrastructure.Identity;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Interceptors;
using Library.Infrastructure.Persistence.Repositories;
using Library.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Playground.Mvc.Clients;
using Playground.Mvc.Handlers;
using Playground.Mvc.Jobs;
using Playground.Mvc.Options;
using Playground.Mvc.Services;

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<AuditInterceptor>();

builder.Services.AddDbContext<LibraryDbContext>((serviceProvider, options) =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryDb"))
    .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>()));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateAuthorCommand).Assembly));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICategoryCacheService, CategoryCacheService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<LibraryDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IRequestTracker, RequestTracker>();

builder.Services.AddOptions<LibraryApiOptions>()
    .Bind(builder.Configuration.GetSection(LibraryApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Enregistrement - Type Client + Delegating Handler + Polly
builder.Services.AddTransient<LoggingDelegatingHandler>();

builder.Services.AddHttpClient<ILibraryApiSyncClient, LibraryApiSyncClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<LibraryApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
.AddHttpMessageHandler<LoggingDelegatingHandler>()
.AddResilienceHandler("library-api-pipeline", pipeline =>
{
    pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => !r.IsSuccessStatusCode)
    });

    pipeline.AddTimeout(TimeSpan.FromSeconds(5));

    pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(20),
        MinimumThroughput = 4,
        BreakDuration = TimeSpan.FromSeconds(15),
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => !r.IsSuccessStatusCode)
    });
});

// Setup config Quartz.Net
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("AuthorSyncJob");
    q.AddJob<AuthorSyncJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("AuthorSyncJob-trigger")
        .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(45)).RepeatForever()));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    var routes = endpointSources
        .SelectMany(source => source.Endpoints)
        .OfType<RouteEndpoint>()
        .Select(e => e.RoutePattern.RawText);
    return Results.Ok(routes);
});

app.Run();
