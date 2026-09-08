// src/Library.Application/Auth/Commands/Logout/LogoutCommand.cs
using MediatR;

namespace Library.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<LogoutResult>;

public record LogoutResult(bool Success, string? ErrorMessage);