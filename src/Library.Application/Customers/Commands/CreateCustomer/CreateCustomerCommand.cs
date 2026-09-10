using Library.Domain.ValueObjects;
using MediatR;

namespace Library.Application.Customers.Commands.CreateCustomer
{
    public record CreateCustomerCommand(string Name, Guid CompanyId, Money CreditLimit) : IRequest<CreateCustomerResult>;

    public record CreateCustomerResult(bool Success, Guid? CustomerId, string? ErrorMessage);
}
