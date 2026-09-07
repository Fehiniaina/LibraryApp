using Library.Infrastructure.Persistence;
using Library.Application.Authors.Common;
using Library.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Authors.Queries.SearchAuthors;

public class SearchAuthorsQueryHandler : IRequestHandler<SearchAuthorsQuery, PagedResult<AuthorDto>>
{
    private readonly LibraryDbContext _db;
    public SearchAuthorsQueryHandler(LibraryDbContext db) => _db = db;

    public async Task<PagedResult<AuthorDto>> Handle(SearchAuthorsQuery request, CancellationToken ct)
    {
        const int MaxPageSize = 100;
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(request.Page, 1);

        IQueryable<Author> query = _db.Authors.AsNoTracking();

        if (!string.IsNullOrEmpty(request.LastName))
        {
            query = query.Where(author => author.LastName.Contains(request.LastName));
        }

        if (request.MinBookCount.HasValue && request.MinBookCount > 0)
        {
            query = query.Where(author => author.Books.Count >= request.MinBookCount);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuthorDto(a.Id, a.FirstName, a.LastName, a.Books.Count))
            .ToListAsync(ct);

        return new PagedResult<AuthorDto>(items, totalCount, page, pageSize);
    }
}