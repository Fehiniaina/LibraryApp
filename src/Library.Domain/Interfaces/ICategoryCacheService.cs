using Library.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Domain.Interfaces
{
    public interface ICategoryCacheService
    {
        ValueTask<List<Category>> GetAllCategoriesAsync(CancellationToken ct);
    }
}
