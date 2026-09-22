using System.Security.Claims;

using Microsoft.AspNetCore;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

internal static class TokenEndpoints
{
    public static void MapTokenEndpoints(this WebApplication app)
    {
        app.MapPost("/connect/token", async (HttpContext httpContext) =>
        {
            var request = httpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("The OpenIddict request cannot be retrieved.");

            if (!request.IsClientCredentialsGrantType())
            {
                throw new NotImplementedException("Only the client_credentials grant is supported.");
            }

            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            identity.SetClaim(Claims.Subject, request.ClientId);
            identity.SetClaim(Claims.Name, request.ClientId);
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(_ => new[] { Destinations.AccessToken });

            return Results.SignIn(new ClaimsPrincipal(identity), authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        });
    }
}