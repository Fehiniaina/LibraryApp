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
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuthorDto(a.Id, a.FirstName, a.LastName, a.Books.Count))
            .ToListAsync(ct);

        return new PagedResult<AuthorDto>(items, totalCount, request.Page, request.PageSize);
    }
}