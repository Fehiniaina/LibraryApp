namespace Library.Domain.Interfaces;

using Library.Domain.Entities;

public interface ICategoryCacheService
{
    ValueTask<List<Category>> GetAllCategoriesAsync(CancellationToken ct);
}
