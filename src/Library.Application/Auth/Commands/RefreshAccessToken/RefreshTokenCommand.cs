// src/Library.Application/Auth/Commands/RefreshToken/RefreshTokenCommand.cs
using Library.Application.Auth.Commands.Login;
using MediatR;

namespace Library.Application.Auth.Commands.RefreshAccessToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResult>;