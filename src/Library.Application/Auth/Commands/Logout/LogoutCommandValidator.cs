// src/Library.Application/Auth/Commands/Logout/LogoutCommandValidator.cs
namespace Library.Application.Auth.Commands.Logout;

using FluentValidation;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Le refresh token est obligatoire.");
    }
}