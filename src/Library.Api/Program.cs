using FluentValidation;
using Library.Application.Authors.Commands.CreateAuthor;
using Library.Application.Authors.Commands.UpdateAuthor;
using Library.Application.Authors.Commands.CreateAuthorWithBook;
using Library.Application.Authors.Common;
using Library.Application.Authors.Queries.GetAllAuthors;
using Library.Application.Authors.Queries.GetAuthorExplicit;
using Library.Application.Authors.Queries.GetAuthorsEager;
using Library.Application.Authors.Queries.GetAuthorsLazy;
using Library.Application.Authors.Queries.SearchAuthors;
using Library.Application.Books.Queries.SearchExpensive;
using Library.Application.Common.Behaviors;
using Library.Api.Middleware;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryDbContext>(options =>
    // options.UseLazyLoadingProxies() // active le lazy loading
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
    await LibrarySeeder.SeedAsync(
        db,
        authorCount: 400,
        categoryCount: 50,
        forceReset: forceReset
    );
}

// app.UseHttpsRedirection(); // désactivé pour tests HTTP locaux

app.MapPost("/authors", async (IMediator mediator, CreateAuthorCommand command) =>
{
    var id = await mediator.Send(command);
    return Results.Created($"/authors/{id}", id);
});

app.MapGet("/authors", async (IMediator mediator) =>
    await mediator.Send(new GetAllAuthorsQuery()));

app.MapGet("/authors/eager", async (IMediator mediator) =>
    await mediator.Send(new GetAuthorsEagerQuery()));

app.MapGet("/authors/{id}/explicit", async (IMediator mediator, Guid id) =>
{
    var result = await mediator.Send(new GetAuthorExplicitQuery(id));
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapGet("/authors/lazy", async(IMediator mediator) =>
    await mediator.Send(new GetAuthorsLazyQuery()));

app.MapGet("/authors/search", async (
    IMediator mediator,
    string? lastName,
    int? minBookCount,
    int page = 1,
    int pageSize = 20) =>
{
    var result = await mediator.Send(new SearchAuthorsQuery(lastName, minBookCount, page, pageSize));
    return Results.Ok(result);
});

app.MapPut("/authors/{id}", async (IMediator mediator, Guid id, UpdateAuthorDto dto) =>
{
    var result = await mediator.Send(new UpdateAuthorCommand(id, dto.FirstName, dto.LastName));
    
    return result switch
    {
        UpdateAuthorResult.Success => Results.NoContent(),
        UpdateAuthorResult.NotFound => Results.NotFound(),
        UpdateAuthorResult.Conflict => Results.Conflict("Cet auteur a été modifié par quelqu'un d'autre. Rechargez et réessayez."),
        _ => Results.Problem()
    };
});

app.MapPost("/authors/with-book", async (IMediator mediator, CreateAuthorWithBookCommand command) =>
{
    var result = await mediator.Send(command);
    return result.Success
        ? Results.Created($"/authors/{result.AuthorId}", result)
        : Results.Conflict(result.ErrorMessage);
});

app.MapGet("/book/search", async(
    IMediator mediator,
    decimal Price,
    int page = 1,
    int pageSize = 20) => 
{
    var result = await mediator.Send(new SearchExpensiveQuery(Price, page, pageSize));

    return Results.Ok(result);
});

app.Run();