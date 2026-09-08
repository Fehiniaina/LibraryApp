// src/Library.Application/Authors/Queries/GetAuthorsLazy/GetAuthorsLazyQueryHandler.cs
using Library.Application.Authors.Common;
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Authors.Queries.GetAuthorsLazy;

public class GetAuthorsLazyQueryHandler : IRequestHandler<GetAuthorsLazyQuery, List<AuthorWithBooksDto>>
{
    private readonly LibraryDbContext _db;
    public GetAuthorsLazyQueryHandler(LibraryDbContext db) => _db = db;

    public async Task<List<AuthorWithBooksDto>> Handle(GetAuthorsLazyQuery request, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Une seule requête ici — en apparence anodin
        // Lazy loading ne fonctionne pas avec AsNoTracking() contrairement a Eager
        var authors = await _db.Authors.ToListAsync(ct);

        var result = new List<AuthorWithBooksDto>();
        foreach (var author in authors)
        {
            // Chaque accès à author.Books déclenche ICI une nouvelle requête SQL,
            // invisible à la lecture du code — c'est tout le piège du lazy loading
            result.Add(new AuthorWithBooksDto(
                author.Id, author.FirstName, author.LastName,
                author.Books.Select(b => b.Title).ToList()
            ));
        }

        sw.Stop();
        Console.WriteLine($">>> [LAZY] {sw.ElapsedMilliseconds}ms, {result.Count} authors, {result.Sum(a => a.BookTitles.Count)} books total");

        return result;
    }
}