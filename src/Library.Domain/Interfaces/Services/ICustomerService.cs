namespace Library.Domain.Interfaces.Services;

using Library.Domain.Entities;
using Library.Domain.ValueObjects;

public interface ICustomerService
{
    Task<Customer> CreateCustomerAsync(string name, Guid companyId, Money creditLimit, CancellationToken ct);
}
