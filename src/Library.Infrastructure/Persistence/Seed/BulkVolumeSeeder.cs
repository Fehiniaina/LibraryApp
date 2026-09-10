using Bogus;
using Library.Domain.Entities;
using Library.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence.Seed;

public static class BulkVolumeSeeder
{
    public static async Task SeedLargeVolumeAsync(LibraryDbContext db, int authorCount = 10000)
    {
        var existingCount = await db.Authors.CountAsync();

        if (existingCount < authorCount)
        {
            var remainingToCreate = authorCount - existingCount;
            Console.WriteLine($">>> {existingCount} auteurs déjà présents, création de {remainingToCreate} supplémentaires.");

            var authorFaker = new Faker<Author>()
                .CustomInstantiator(f => new Author(f.Name.FirstName(), f.Name.LastName()));

            var authors = authorFaker.Generate(remainingToCreate);
            const int batchSize = 1000;

            for (int i = 0; i < authors.Count; i += batchSize)
            {
                var batch = authors.Skip(i).Take(batchSize).ToList();
                db.Authors.AddRange(batch);
                await db.SaveChangesAsync(); // simple, sans transaction/strategy explicite
                db.ChangeTracker.Clear();
                Console.WriteLine($">>> Auteurs insérés : {i + batch.Count}/{authors.Count}");
            }
        }
        else
        {
            Console.WriteLine($">>> {existingCount} auteurs déjà présents (>= {authorCount}), phase Authors ignorée.");
        }

        var targetBookCount = authorCount * 10;
        var existingBooksCount = await db.Books.CountAsync();

        if (existingBooksCount >= targetBookCount)
        {
            Console.WriteLine($">>> {existingBooksCount} livres déjà présents, phase Books ignorée.");
            return;
        }

        var allAuthors = await db.Authors.AsNoTracking().ToListAsync();
        var remainingBooksToCreate = targetBookCount - existingBooksCount;

        Console.WriteLine($">>> {existingBooksCount} livres déjà présents, création de {remainingBooksToCreate} supplémentaires.");

        var bookFaker = new Faker<Book>()
            .CustomInstantiator(f => new Book(
                f.Commerce.ProductName(),
                f.PickRandom(allAuthors),
                new Price(f.Random.Decimal(5, 100), "EUR")
            ));

        var books = bookFaker.Generate(remainingBooksToCreate);
        const int bookBatchSize = 1000;

        for (int i = 0; i < books.Count; i += bookBatchSize)
        {
            var batch = books.Skip(i).Take(bookBatchSize).ToList();
            db.Books.AddRange(batch);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            Console.WriteLine($">>> Livres insérés : {i + batch.Count}/{books.Count}");
        }
    }
}