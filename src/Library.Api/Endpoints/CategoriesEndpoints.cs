namespace Library.Api.Endpoints;

using Library.Application.Categories.Queries;

using MediatR;

internal static class CategoriesEndpoints
{
    public static void MapCategoriesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/categories")
            .WithTags("Categories")
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator) =>
        {
            await mediator.Send(new GetAllCategoriesQuery()).ConfigureAwait(false);
        });
    }
}