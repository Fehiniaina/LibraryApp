// src/Library.Application/Authors/Commands/UpdateAuthor/UpdateAuthorCommandHandler.cs
namespace Library.Application.Authors.Commands.UpdateAuthor;

using Library.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, UpdateAuthorResult>
{
    private readonly LibraryDbContext _db;

    public UpdateAuthorCommandHandler(LibraryDbContext db) => _db = db;

    public async Task<UpdateAuthorResult> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
    {
        // Pas de AsNoTracking ici — on VEUT que le Change Tracker suive cette entité
        var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (author is null)
        {
            return UpdateAuthorResult.NotFound;
        }

        author.UpdateName(request.FirstName, request.LastName);

        // Aucun appel explicite à _db.Authors.Update(author) — pourquoi ça marche quand même ?
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return UpdateAuthorResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return UpdateAuthorResult.Conflict;
        }
    }
}

/// <summary>
/// Types of status.
/// </summary>
public enum UpdateAuthorResult
{
    /// <summary>
    /// Represents a Success.
    /// </summary>
    Success,

    /// <summary>
    /// Represents a Not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// Represents a Conflict.
    /// </summary>
    Conflict,
}