// Authors/Queries/GetAuthorExplicit/GetAuthorExplicitQueryHandler.cs
namespace Library.Application.Authors.Queries.GetAuthorExplicit;

using Library.Infrastructure.Persistence;
using Library.Application.Authors.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetAuthorExplicitQueryHandler : IRequestHandler<GetAuthorExplicitQuery, AuthorWithBooksDto?>
{
    private readonly LibraryDbContext _db;
    public GetAuthorExplicitQueryHandler(LibraryDbContext db) => _db = db;

    public async Task<AuthorWithBooksDto?> Handle(GetAuthorExplicitQuery request, CancellationToken cancellationToken)
    {
        // 1ère requête — l'auteur seul, SANS ses livres
        var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == request.AuthorId, cancellationToken);
        if (author is null) return null;

        // 2ème requête — explicite, déclenchée manuellement, uniquement si on en a besoin
        await _db.Entry(author)
            .Collection(a => a.Books)
            .LoadAsync(cancellationToken);

        return new AuthorWithBooksDto(
            author.Id, author.FirstName, author.LastName,
            author.Books.Select(b => b.Title).ToList()
        );
    }
}