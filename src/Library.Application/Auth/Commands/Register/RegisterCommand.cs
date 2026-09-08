// src/Library.Application/Auth/Commands/Register/RegisterCommand.cs
using MediatR;

namespace Library.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<RegisterResult>;

public record RegisterResult(bool Success, string? ErrorMessage);