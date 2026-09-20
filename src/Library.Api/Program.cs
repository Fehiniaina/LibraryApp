using System.Text;
using System.Text.Json;

using FluentValidation;

using Library.Api.Endpoints;
using Library.Api.Middleware;
using Library.Api.Services;
using Library.Application.Authors.Commands.CreateAuthor;
using Library.Application.Common.Behaviors;
using Library.Application.Customers.EventHandlers;
using Library.Domain.Interfaces;
using Library.Domain.Interfaces.Services;
using Library.Infrastructure.BackgroundServices;
using Library.Infrastructure.ExternalServices;
using Library.Infrastructure.HealthChecks;
using Library.Infrastructure.Identity;
using Library.Infrastructure.Jobs;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Interceptors;
using Library.Infrastructure.Persistence.Repositories;
using Library.Infrastructure.Persistence.Seed;
using Library.Infrastructure.Services;

using MassTransit;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

using Quartz;

using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AuditInterceptor>(); // singleton

builder.Services.AddDbContext<LibraryDbContext>((serviceProvider, options) =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LibraryDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 0))
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
        ClockSkew = TimeSpan.Zero, // évite une tolérance de temps par défaut de 5min — best practice pour les tokens courts
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSingleton<INotificationPublisher, ResilientNotificationPublisher>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateAuthorCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.NotificationPublisherType = typeof(ResilientNotificationPublisher);
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
        Scheme = "Bearer",
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
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

    // q.AddTrigger(opts => opts
    // .ForJob(jobKey)
    // .WithIdentity("RefreshTokenCleanupJob-trigger")
    // .WithSimpleSchedule(x => x
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
        },
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
        },
    });
});

// Lire une fichier de Configuration
builder.Services.Configure<ExternalStatusApiOptions>(
    builder.Configuration.GetSection(ExternalStatusApiOptions.SectionName));

builder.Services.AddOptions<ExternalStatusApiOptions>()
    .Bind(builder.Configuration.GetSection(ExternalStatusApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart(); // échoue au DÉMARRAGE de l'app si la config est invalide, pas au premier usage

// Register respository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICategoryCacheService, CategoryCacheService>();

builder.Services.AddHostedService<OutboxProcessorService>();

// Setting up mass transit services
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// Setting up health check for rabbitmq
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@rabbitmq:5672"),
        AutomaticRecoveryEnabled = true,
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// Setting up health check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LibraryDbContext>(name: "sqlserver")
    .AddRabbitMQ(name: "rabbitmq")
    .AddCheck<OutboxHealthCheck>("outbox-processing");

// Setting up gRPC
builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(options =>
{
    // Port HTTP/1.1 classique
    options.ListenAnyIP(8080, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);

    // Port DÉDIÉ HTTP/2 — pour gRPC uniquement
    options.ListenAnyIP(8082, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGrpcService<CustomerGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.MigrateAsync().ConfigureAwait(false);

    var forceReset = args.Contains("--reset-seed");
    await LibrarySeeder.SeedAsync(db, authorCount: 20, categoryCount: 50, forceReset: forceReset);
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
            }),
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    },
});

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapAuthorEndpoints();
app.MapBookEndpoints();
app.MapExternalEndpoints();
app.MapDebugEndpoints();
app.MapCustomerEndpoints();
app.MapCategoriesEndpoints();
app.MapCompaniesEndpoints();

app.Run();