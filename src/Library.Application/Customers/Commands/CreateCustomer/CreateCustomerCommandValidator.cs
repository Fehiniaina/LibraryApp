using FluentValidation;

namespace Library.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Le nom est obligatoire")
                .MaximumLength(50);

            RuleFor(x => x.CreditLimit.Amount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Le prix ne peut pas être négatif.");

            RuleFor(x => x.CreditLimit.Currency)
                .Length(3)
                .WithMessage("La devise doit être un code ISO à 3 lettres (ex: EUR).");
        }
    }
}
