// src/Library.Application/Authors/Commands/CreateAuthorWithBook/CreateAuthorWithBookCommandValidator.cs
using FluentValidation;

namespace Library.Application.Authors.Commands.CreateAuthorWithBook;

public class CreateAuthorWithBookCommandValidator : AbstractValidator<CreateAuthorWithBookCommand>
{
    public CreateAuthorWithBookCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prénom est obligatoire.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom est obligatoire.")
            .MaximumLength(100);

        RuleFor(x => x.BookTitle)
            .NotEmpty().WithMessage("Le titre du livre est obligatoire.")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix ne peut pas être négatif.");

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("La devise doit être un code ISO à 3 lettres (ex: EUR).");
    }
}