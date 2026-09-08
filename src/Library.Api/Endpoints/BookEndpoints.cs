// src/Library.Api/Endpoints/BookEndpoints.cs
using Library.Application.Books.Queries.SearchExpensive;
using MediatR;

namespace Library.Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/books")
            .WithTags("Books")
            .RequireAuthorization();

        group.MapGet("/search", async (
            IMediator mediator,
            decimal price,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await mediator.Send(new SearchExpensiveQuery(price, page, pageSize));
            return Results.Ok(result);
        });
    }
}