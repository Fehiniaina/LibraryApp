using Library.Application.Authors.Common;
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Authors.Queries.GetAllAuthors;

public class GetAllAuthorsQueryHandler : IRequestHandler<GetAllAuthorsQuery, List<AuthorDto>>
{
    private readonly LibraryDbContext _db;

    public GetAllAuthorsQueryHandler(LibraryDbContext db) => _db = db;

    public async Task<List<AuthorDto>> Handle(GetAllAuthorsQuery request, CancellationToken cancellationToken)
    {
        // Best practice #1 : AsNoTracking sur les requêtes en lecture seule
        // Best practice #2 : projection directe en DTO via Select, pas de chargement d'entité complète
        return await _db.Authors
            .AsNoTracking()
            .Select(a => new AuthorDto(a.Id, a.FirstName, a.LastName, a.Books.Count))
            .ToListAsync(cancellationToken);
    }
}