// src/Library.Application/Auth/Commands/Logout/LogoutCommandValidator.cs
using FluentValidation;

namespace Library.Application.Auth.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Le refresh token est obligatoire.");
    }
}