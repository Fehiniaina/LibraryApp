using FluentValidation;

namespace Library.Application.Books.Queries.SearchExpensive;

public class SearchExpensiveValidator : AbstractValidator<SearchExpensiveQuery>
{
    public SearchExpensiveValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix ne peut pas être négatif.");
    }
}