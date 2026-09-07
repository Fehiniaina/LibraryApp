// src/Library.Api/Endpoints/AuthorEndpoints.cs
using Library.Application.Authors.Commands.CreateAuthor;
using Library.Application.Authors.Commands.CreateAuthorWithBook;
using Library.Application.Authors.Commands.UpdateAuthor;
using Library.Application.Authors.Queries.GetAllAuthors;
using Library.Application.Authors.Queries.GetAuthorExplicit;
using Library.Application.Authors.Queries.GetAuthorsEager;
using Library.Application.Authors.Queries.GetAuthorsLazy;
using Library.Application.Authors.Queries.SearchAuthors;
using MediatR;

namespace Library.Api.Endpoints;

public static class AuthorEndpoints
{
    public static void MapAuthorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/authors").WithTags("Authors");

        group.MapPost("/", async (IMediator mediator, CreateAuthorCommand command) =>
        {
            var id = await mediator.Send(command);
            return Results.Created($"/authors/{id}", id);
        });

        group.MapGet("/", async (IMediator mediator) =>
            await mediator.Send(new GetAllAuthorsQuery()));

        group.MapGet("/eager", async (IMediator mediator) =>
            await mediator.Send(new GetAuthorsEagerQuery()));

        group.MapGet("/{id}/explicit", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetAuthorExplicitQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapGet("/lazy", async (IMediator mediator) =>
            await mediator.Send(new GetAuthorsLazyQuery()));

        group.MapGet("/search", async (
            IMediator mediator,
            string? lastName,
            int? minBookCount,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await mediator.Send(new SearchAuthorsQuery(lastName, minBookCount, page, pageSize));
            return Results.Ok(result);
        });

        group.MapPut("/{id}", async (IMediator mediator, Guid id, UpdateAuthorDto dto) =>
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

        group.MapPost("/with-book", async (IMediator mediator, CreateAuthorWithBookCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.Success
                ? Results.Created($"/authors/{result.AuthorId}", result)
                : Results.Conflict(result.ErrorMessage);
        });
    }
}