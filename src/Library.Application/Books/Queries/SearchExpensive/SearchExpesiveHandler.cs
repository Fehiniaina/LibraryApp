using Library.Application.Authors.Common;
using Library.Application.Books.Common;
using Library.Infrastructure.Persistence;
using Library.Application.Shared;
using Library.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Books.Queries.SearchExpensive;

public class SearchExpensiveHandler : IRequestHandler<SearchExpensiveQuery, PagedResult<BookDto>>
{
    private readonly LibraryDbContext _db;
    public SearchExpensiveHandler(LibraryDbContext db) => _db = db;

    public async Task<PagedResult<BookDto>> Handle(SearchExpensiveQuery request, CancellationToken ct)
    {
        const int MaxPageSize = 100;
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(request.Page, 1);

        IQueryable<Book> query = _db.Books.AsNoTracking();

        if (request.Price > 0)
        {
            query = query.Where(o => o.Price.Amount >= request.Price);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(o => o.Price.Amount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new BookDto(o.Id, o.Title, o.Author.FirstName + " " + o.Author.LastName, o.Price.Amount))
            .ToListAsync(ct);

        return new PagedResult<BookDto>(items, totalCount, page, pageSize);
    }
}
