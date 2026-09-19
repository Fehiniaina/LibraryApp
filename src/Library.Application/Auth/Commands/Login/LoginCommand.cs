// src/Library.Application/Auth/Commands/Login/LoginCommand.cs
namespace Library.Application.Auth.Commands.Login;

using MediatR;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(bool Success, string? AccessToken, string? RefreshToken, string? ErrorMessage);