// src/Library.Application/Authors/Commands/UpdateAuthor/UpdateAuthorCommandHandler.cs
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Authors.Commands.UpdateAuthor;

public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, UpdateAuthorResult>
{
    private readonly LibraryDbContext _db;
    public UpdateAuthorCommandHandler(LibraryDbContext db) => _db = db;

    public async Task<UpdateAuthorResult> Handle(UpdateAuthorCommand request, CancellationToken ct)
    {
        // Pas de AsNoTracking ici — on VEUT que le Change Tracker suive cette entité
        var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == request.Id, ct);
        if (author is null) return UpdateAuthorResult.NotFound;

        author.UpdateName(request.FirstName, request.LastName);
        // Aucun appel explicite à _db.Authors.Update(author) — pourquoi ça marche quand même ?

        try
        {
            await _db.SaveChangesAsync(ct);
            return UpdateAuthorResult.Success;
        }
        catch(DbUpdateConcurrencyException)
        {
            return UpdateAuthorResult.Conflict;
        }
        
    }
}


public enum UpdateAuthorResult
{
    Success,
    NotFound,
    Conflict
}