namespace Library.Api.Endpoints;

using Library.Application.Authors.Common;
using Library.Domain.Common;
using Library.Domain.Events.Customers;
using Library.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Classe statique enregistrée via extension method — jamais instanciée directement.")]
internal static class DebugEndpoints
{
    public static void MapDebugEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/debug").WithTags("Debug");

        // Cas 1 — succès attendu (un seul résultat)
        group.MapGet("/single-author/{lastName}", async (LibraryDbContext db, string lastName) =>
        {
            var authors = await db.Authors
                .Where(a => a.LastName == lastName)
                .ToListAsync()
                .ConfigureAwait(false);

            try
            {
                var author = authors.GetSingleOrThrow($"Aucun auteur unique trouvé pour le nom '{lastName}'.");
                return Results.Ok(new { author.Id, author.FirstName, author.LastName });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        // Exercice 2.1 : Écris une requête qui retourne, pour chaque auteur, son nom complet ("Prénom Nom") et le titre de son livre le plus cher.
        group.MapGet("/exercice/2/1", async (LibraryDbContext db) =>
        {
            return await db.Authors
                .AsNoTracking()
                .Select(a => new AuthorTopBookDto(
                        a.FirstName + " " + a.LastName,
                        a.Books.Any() ? a.Books.OrderByDescending(b => b.Price.Amount).First().Title : null))
                .ToListAsync()
                .ConfigureAwait(false);
        });

        group.MapGet("/exercice/2/2", async (LibraryDbContext db) =>
        {
            var titles = await db.Authors
                .Where(a => a.LastName.StartsWith('H'))
                .SelectMany(a => a.Books.Select(b => b.Title))
                .ToListAsync()
                .ConfigureAwait(false);

            return titles;
        });

        group.MapGet("/exercice/2/3", async (LibraryDbContext db) =>
        {
            var titles = await db.Categories
                .Select(c => new { c.Name, BookCount = c.Books.Count })
                .OrderByDescending(c => c.BookCount)
                .ToListAsync()
                .ConfigureAwait(false);

            return titles;
        });

        group.MapGet("/exercice/2/4", async (LibraryDbContext db) =>
        {
            var result = await db.Authors
                .Where(a => a.Books.Count > 5)
                .OrderByDescending(a => a.Books.Count)
                .Take(10)
                .Select(a => new AuthorDto(a.Id, a.FirstName, a.LastName, a.Books.Count))
                .ToListAsync()
                .ConfigureAwait(false);
        });

        group.MapGet("/exercice/2/5", async (LibraryDbContext db) =>
        {
            var result = await db.Authors
                .GroupBy(a => a.LastName.Substring(0, 1))
                .Select(a => new { FirstLetter = a.Key, Count = a.Count() })
                .OrderBy(a => a.FirstLetter)
                .ToListAsync()
                .ConfigureAwait(false);

            return result;
        });

        app.MapGet("/handlers", (IServiceProvider sp) =>
        {
            var handlers = sp.GetServices<INotificationHandler<CustomerCreatedEvent>>();
            return handlers.Select(h => h.GetType().Name).ToList();
        });
    }
}

sealed record AuthorTopBookDto(string FullName, string? MostExpensiveBookTitle);