
// src/Library.Api/Endpoints/AuthEndpoints.cs
using Library.Application.Auth.Commands.Login;
using Library.Application.Auth.Commands.Register;
using MediatR;

namespace Library.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async (IMediator mediator, RegisterCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.Success ? Results.Ok() : Results.BadRequest(result.ErrorMessage);
        });

        group.MapPost("/login", async (IMediator mediator, LoginCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.Success ? Results.Ok(new { result.AccessToken, result.RefreshToken }) : Results.Unauthorized();
        });
    }
}