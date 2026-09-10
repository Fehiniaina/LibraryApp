using Library.Domain.Entities;
using Library.Domain.ValueObjects;

namespace Library.Domain.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<Customer> CreateCustomerAsync(string name, Guid companyId, Money creditLimit, CancellationToken ct);
    }
}
