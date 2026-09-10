using Library.Domain.Entities;

namespace Library.Domain.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<List<Customer>> GetByCompanyIdAsync(Guid companyId, CancellationToken ct = default);
        Task<Customer?> GetWithCompanyAsync(Guid customerId, CancellationToken ct = default);
    }
}
