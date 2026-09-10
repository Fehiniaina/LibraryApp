using Library.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly LibraryDbContext _db;
        protected readonly DbSet<T> _dbSet;
        
        public Repository(LibraryDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

        public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
            => await _dbSet.ToListAsync(ct);

        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await _dbSet.AddAsync(entity, ct);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Remove(T entity) => _dbSet.Remove(entity);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
