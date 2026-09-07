// Authors/Queries/GetAuthorExplicit/GetAuthorExplicitQueryHandler.cs
using Library.Infrastructure.Persistence;
using Library.Application.Authors.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Authors.Queries.GetAuthorExplicit;

public class GetAuthorExplicitQueryHandler : IRequestHandler<GetAuthorExplicitQuery, AuthorWithBooksDto?>
{
    private readonly LibraryDbContext _db;
    public GetAuthorExplicitQueryHandler(LibraryDbContext db) => _db = db;

    public async Task<AuthorWithBooksDto?> Handle(GetAuthorExplicitQuery request, CancellationToken ct)
    {
        // 1ère requête — l'auteur seul, SANS ses livres
        var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == request.AuthorId, ct);
        if (author is null) return null;

        // 2ème requête — explicite, déclenchée manuellement, uniquement si on en a besoin
        await _db.Entry(author)
            .Collection(a => a.Books)
            .LoadAsync(ct);

        return new AuthorWithBooksDto(
            author.Id, author.FirstName, author.LastName,
            author.Books.Select(b => b.Title).ToList()
        );
    }
}