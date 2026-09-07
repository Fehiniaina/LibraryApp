using Library.Infrastructure.Persistence;
using Library.Application.Authors.Commands.CreateAuthor;
using Library.Application.Authors.Queries.GetAllAuthors;
using Library.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryDb"))
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAuthorCommand).Assembly));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.MigrateAsync();
    await LibrarySeeder.SeedAsync(db, authorCount: 400);
}

// app.UseHttpsRedirection(); // désactivé pour tests HTTP locaux

app.MapPost("/authors", async (IMediator mediator, CreateAuthorCommand command) =>
{
    var id = await mediator.Send(command);
    return Results.Created($"/authors/{id}", id);
});

app.MapGet("/authors", async (IMediator mediator) =>
    await mediator.Send(new GetAllAuthorsQuery()));

app.Run();