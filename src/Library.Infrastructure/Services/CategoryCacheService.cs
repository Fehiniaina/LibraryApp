using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Library.Infrastructure.Services;

public class CategoryCacheService(LibraryDbContext db, IMemoryCache cache) : ICategoryCacheService, IDisposable
{
    private const string CacheKey = "all-categories";

    private readonly SemaphoreSlim _lock = new(1, 1);

    private bool disposedValue;

    public async ValueTask<List<Category>> GetAllCategoriesAsync(CancellationToken ct)
    {
        // Cas SYNCHRONE — déjà en cache, aucune allocation Task nécessaire
        if (cache.TryGetValue(CacheKey, out List<Category>? cached) && cached is not null)
        {
            return cached; // pas d'allocation Heap — struct
        }

        await _lock.WaitAsync(ct);
        try 
        {
            // 3. RE-CHECK après le verrou — un autre thread a peut-être déjà rempli le cache entre-temps
            if (cache.TryGetValue(CacheKey, out cached) && cached is not null)
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
        var categories = await db.Categories.AsNoTracking().ToListAsync(ct);

        cache.Set(CacheKey, categories, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return categories;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _lock.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~CategoryCacheService()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
