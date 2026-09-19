namespace Library.Application.Books.Queries.SearchExpensive;

using Library.Application.Books.Common;
using Library.Application.Shareds;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

public class SearchExpensiveHandler : IRequestHandler<SearchExpensiveQuery, PagedResult<BookDTO>>
{
    private readonly LibraryDbContext _db;

    public SearchExpensiveHandler(LibraryDbContext db) => _db = db;

    public async Task<PagedResult<BookDTO>> Handle(SearchExpensiveQuery request, CancellationToken cancellationToken)
    {
        const int MaxPageSize = 100;
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(request.Page, 1);

        IQueryable<Book> query = _db.Books.AsNoTracking();

        query = request.Price switch
        {
            > 0 => query.Where(o => o.Price.Amount >= request.Price),
            _ => query
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(o => o.Price.Amount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new BookDTO(o.Id, o.Title, o.Author.FirstName + " " + o.Author.LastName, o.Price.Amount))
            .ToListAsync(cancellationToken);

        return new PagedResult<BookDTO>(items, totalCount, page, pageSize);
    }
}
