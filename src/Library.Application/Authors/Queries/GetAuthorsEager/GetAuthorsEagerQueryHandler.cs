// Authors/Queries/GetAuthorsEager/GetAuthorsEagerQueryHandler.cs
using Library.Infrastructure.Persistence;
using Library.Application.Authors.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Authors.Queries.GetAuthorsEager;

public class GetAuthorsEagerQueryHandler : IRequestHandler<GetAuthorsEagerQuery, List<AuthorWithBooksDto>>
{

    // Eager Loading — Include/ThenInclude
    private readonly LibraryDbContext _db;
    public GetAuthorsEagerQueryHandler(LibraryDbContext db) => _db = db;

    public async Task<List<AuthorWithBooksDto>> Handle(GetAuthorsEagerQuery request, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var authors = await _db.Authors
            .AsNoTracking()
            .Include(a => a.Books)
            .ToListAsync(ct);

        var result = authors.Select(a => new AuthorWithBooksDto(
            a.Id, a.FirstName, a.LastName,
            a.Books.Select(b => b.Title).ToList()
        )).ToList();

        sw.Stop();
        Console.WriteLine($">>> [EAGER] {sw.ElapsedMilliseconds}ms, {result.Count} authors, {result.Sum(a => a.BookTitles.Count)} books total");

        return result;
    }
}