// src/Library.Infrastructure/Persistence/Seed/LibrarySeeder.cs
using Bogus;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence.Seed;

public static class LibrarySeeder
{
    public static async Task SeedAsync(LibraryDbContext db, int authorCount = 50, int categoryCount = 10)
    {
        Console.WriteLine(">>> SeedAsync called");

        if (await db.Authors.AnyAsync()) // évite de reseeder si déjà peuplé
        {
            Console.WriteLine(">>> Authors already exist, skipping seed");
            return;
        }

        // Catégories
        var categoryFaker = new Faker<Category>()
            .CustomInstantiator(f => new Category(f.Commerce.Categories(1)[0]));

        var categories = categoryFaker.Generate(categoryCount)
            .DistinctBy(c => c.Name) // évite les doublons de noms générés
            .ToList();

        db.Categories.AddRange(categories);

        // Auteurs + leurs livres
        var authorFaker = new Faker<Author>()
            .CustomInstantiator(f => new Author(f.Name.FirstName(), f.Name.LastName()));

        var authors = authorFaker.Generate(authorCount);

        var bookFaker = new Faker<Book>()
            .CustomInstantiator(f => new Book(f.Commerce.ProductName(), f.PickRandom(authors)));

        var books = bookFaker.Generate(authorCount * 10); // ~10 livres par auteur en moyenne

        foreach (var book in books)
        {
            var randomCategories = new Faker().PickRandom(categories, new Faker().Random.Int(1, 3));
            foreach (var category in randomCategories)
                book.AddCategory(category);
        }

        db.Authors.AddRange(authors);
        db.Books.AddRange(books);

        Console.WriteLine($">>> About to save {authors.Count} authors, {books.Count} books, {categories.Count} categories");
        await db.SaveChangesAsync();
        Console.WriteLine(">>> SaveChangesAsync completed successfully");
    }
}