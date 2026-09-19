using Library.Application.Companies.Queries;

using MediatR;

namespace Library.Api.Endpoints;

internal static class CompaniesEndpoints
{
    public static void MapCompaniesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/companies")
            .WithTags("Companies")
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator) =>
        {
            return await mediator.Send(new GetAllCompaniesQuery());
        });
    }
}
