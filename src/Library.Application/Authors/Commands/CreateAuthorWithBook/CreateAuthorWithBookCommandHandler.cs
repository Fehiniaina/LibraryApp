// src/Library.Application/Authors/Commands/CreateAuthorWithBook/CreateAuthorWithBookCommandHandler.cs
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Library.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Authors.Commands.CreateAuthorWithBook;

public class CreateAuthorWithBookCommandHandler : IRequestHandler<CreateAuthorWithBookCommand, CreateAuthorWithBookResult>
{
    private readonly LibraryDbContext _db;
    public CreateAuthorWithBookCommandHandler(LibraryDbContext db) => _db = db;

    public async Task<CreateAuthorWithBookResult> Handle(CreateAuthorWithBookCommand request, CancellationToken ct)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // Étape 1 — insérer l'auteur, on a besoin de son Id persisté pour la suite de la logique
            var author = new Author(request.FirstName, request.LastName);
            _db.Authors.Add(author);
            await _db.SaveChangesAsync(ct);

            // Étape 2 — VÉRIFICATION MÉTIER qui ne peut se faire qu'APRÈS avoir vu l'état réel de la base
            // (ex: un autre auteur a peut-être créé ce même titre entre-temps)
            var titleExists = await _db.Books.AnyAsync(b => b.Title == request.BookTitle, ct);
            if (titleExists)
            {
                // Rollback conditionnel.
                await transaction.RollbackAsync(ct);
                return new CreateAuthorWithBookResult(false, null, $"Le titre '{request.BookTitle}' existe déjà.");
            }

            // Étape 3 — seulement si la vérification passe, on insère le livre
            var book = new Book(request.BookTitle, author, new Price(request.Price, request.Currency));
            _db.Books.Add(book);
            await _db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            return new CreateAuthorWithBookResult(true, author.Id, null);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}