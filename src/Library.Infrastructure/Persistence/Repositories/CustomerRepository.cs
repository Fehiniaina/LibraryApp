using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(LibraryDbContext db) : base(db)
        {
        }

        public async Task<List<Customer>> GetByCompanyIdAsync(Guid companyId, CancellationToken ct = default)
        {
            return await _db.Customers
                .Where(c => c.CompanyId == companyId)
                .ToListAsync(ct);
        }

        public async Task<Customer?> GetWithCompanyAsync(Guid customerId, CancellationToken ct = default)
        {
            return await _db.Customers
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == customerId, ct);
        }
    }
}
