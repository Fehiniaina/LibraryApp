using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading;

namespace Library.Infrastructure.Services
{
    public class CategoryCacheService : ICategoryCacheService
    {
        private const string CacheKey = "all-categories";
        private readonly LibraryDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public CategoryCacheService(LibraryDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async ValueTask<List<Category>> GetAllCategoriesAsync(CancellationToken ct)
        {
            // Cas SYNCHRONE — déjà en cache, aucune allocation Task nécessaire
            if (_cache.TryGetValue(CacheKey, out List<Category>? cached) && cached is not null)
            {
                return cached; // pas d'allocation Heap — struct
            }

            await _lock.WaitAsync(ct);
            try 
            {
                // 3. RE-CHECK après le verrou — un autre thread a peut-être déjà rempli le cache entre-temps
                if (_cache.TryGetValue(CacheKey, out cached) && cached is not null)
                {
                    return cached;
                }

                return await FetchAndCacheAsync(ct);
            } 
            finally 
            { 
                _lock.Release(); 
            }
        }

        private async Task<List<Category>> FetchAndCacheAsync(CancellationToken ct)
        {
            Console.WriteLine(">>> [CACHE MISS] Requête DB déclenchée...");
            var categories = await _db.Categories.AsNoTracking().ToListAsync(ct);

            _cache.Set(CacheKey, categories, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return categories;
        }
    }
}
