using Library.Api.Endpoints;
using Library.Api.Middleware;
using Library.Application.Authors.Commands.CreateAuthor;
using Library.Application.Common.Behaviors;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Seed;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryDb"))
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateAuthorCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateAuthorCommand).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    await LibrarySeeder.SeedAsync(db, authorCount: 400, categoryCount: 50, forceReset: forceReset);
}

app.MapAuthorEndpoints();
app.MapBookEndpoints();

app.Run();