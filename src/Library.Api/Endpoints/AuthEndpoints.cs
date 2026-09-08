
// src/Library.Api/Endpoints/AuthEndpoints.cs
using Library.Application.Auth.Commands.Login;
using Library.Application.Auth.Commands.RefreshAccessToken;
using Library.Application.Auth.Commands.Register;
using Library.Application.Auth.Commands.Logout;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        // AuthEndpoints.cs — ajoute
        group.MapPost("/refresh", async (IMediator mediator, RefreshTokenCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.Success ? Results.Ok(new { result.AccessToken, result.RefreshToken }) : Results.Unauthorized();
        });

        group.MapPost("/logout", async (IMediator mediator, LogoutCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.Success ? Results.NoContent() : Results.BadRequest(result.ErrorMessage);
        }).RequireAuthorization();
    }
}