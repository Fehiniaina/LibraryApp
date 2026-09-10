using Library.Application.Authors.Commands.CreateAuthor;
using Library.Api.Endpoints;
using Library.Api.Middleware;
using Library.Application.Common.Behaviors;
using Library.Domain.Interfaces;
using Library.Infrastructure.Jobs;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Seed;
using Library.Infrastructure.Services;
using Library.Infrastructure.Identity;
using Library.Infrastructure.Persistence.Interceptors;
using Library.Infrastructure.ExternalServices;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Quartz;
using Quartz.Impl;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AuditInterceptor>(); // singleton

builder.Services.AddDbContext<LibraryDbContext>((serviceProvider, options) =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LibraryDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 0)
    )
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>()));

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Best practices — politique de mot de passe
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Verrouillage après tentatives échouées — protection brute-force
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<LibraryDbContext>()
.AddDefaultTokenProviders();

// JWT Bearer Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey non configurée. Utilise 'dotnet user-secrets set'.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero // évite une tolérance de temps par défaut de 5min — best practice pour les tokens courts
    };
});

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateAuthorCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateAuthorCommand).Assembly);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Entre 'Bearer {ton token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Setup quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("RefreshTokenCleanupJob");

    q.AddJob<RefreshTokenCleanupJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("RefreshTokenCleanupJob-trigger")
        .WithCronSchedule("0 0 3 * * ?"));
    //q.AddTrigger(opts => opts
    //.ForJob(jobKey)
    //.WithIdentity("RefreshTokenCleanupJob-trigger")
    //.WithSimpleSchedule(x => x
    //    .WithInterval(TimeSpan.FromSeconds(30))
    //    .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true; // attend la fin des jobs en cours lors d'un arrêt propre de l'app
});

builder.Services.AddTransient<LoggingDelegatingHandler>();

builder.Services.AddHttpClient<IExternalStatusClient, ExternalStatusClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ExternalStatusApiOptions>>().Value;

    client.BaseAddress = new Uri(options.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddHttpMessageHandler<LoggingDelegatingHandler>()
.AddResilienceHandler("external-status-pipeline", pipeline =>
{
    pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => !r.IsSuccessStatusCode),
        OnRetry = args =>
        {
            Console.WriteLine($">>> [RETRY] Tentative {args.AttemptNumber + 1} après échec ({args.Outcome.Result?.StatusCode})");
            return ValueTask.CompletedTask;
        }
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
            .HandleResult(r => !r.IsSuccessStatusCode),
        OnOpened = args =>
        {
            Console.WriteLine(">>> [CIRCUIT BREAKER] Circuit OUVERT — trop d'échecs, arrêt temporaire des tentatives");
            return ValueTask.CompletedTask;
        },
        OnClosed = args =>
        {
            Console.WriteLine(">>> [CIRCUIT BREAKER] Circuit FERMÉ — reprise normale");
            return ValueTask.CompletedTask;
        }
    });
});

// Lire une fichier de Configuration
builder.Services.Configure<ExternalStatusApiOptions>(
    builder.Configuration.GetSection(ExternalStatusApiOptions.SectionName));

builder.Services.AddOptions<ExternalStatusApiOptions>()
    .Bind(builder.Configuration.GetSection(ExternalStatusApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart(); // échoue au DÉMARRAGE de l'app si la config est invalide, pas au premier usage

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.MigrateAsync();

    bool forceReset = args.Contains("--reset-seed");
    await LibrarySeeder.SeedAsync(db, authorCount: 10000, categoryCount: 50, forceReset: forceReset);

    // Seed massif SÉPARÉ — seulement si demandé explicitement, JAMAIS avec --reset-seed en même temps
    if (args.Contains("--bulk-seed"))
    {
        await BulkVolumeSeeder.SeedLargeVolumeAsync(db, authorCount: 10000);
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapAuthorEndpoints();
app.MapBookEndpoints();
app.MapExternalEndpoints();
app.MapDebugEndpoints();

app.Run();