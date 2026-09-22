using Library.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Validation.AspNetCore;

namespace Library.Api.Endpoints;

internal static class ServiceEndpoints
{
    public static void MapServiceEndpoints(this WebApplication app)
    {
        app.MapGet("/service/authors/count", async (LibraryDbContext db) =>
        {
            var count = await db.Authors.CountAsync();

            return Results.Ok(new { count });
        }).RequireAuthorization(policy => policy
            .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
            .RequireClaim("scope", "authors.read"));
    }
}