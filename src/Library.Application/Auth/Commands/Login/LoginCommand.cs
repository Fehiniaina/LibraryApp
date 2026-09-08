// src/Library.Application/Auth/Commands/Login/LoginCommand.cs
using MediatR;

namespace Library.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(bool Success, string? AccessToken, string? RefreshToken, string? ErrorMessage);