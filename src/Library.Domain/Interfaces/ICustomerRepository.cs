namespace Library.Domain.Interfaces;

using Library.Domain.Entities;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<List<Customer>> GetByCompanyIdAsync(Guid companyId, CancellationToken ct = default);

    Task<Customer?> GetWithCompanyAsync(Guid customerId, CancellationToken ct = default);
}
