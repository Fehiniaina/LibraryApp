// src/Library.Infrastructure/Persistence/Seed/LibrarySeeder.cs
using Bogus;

using Library.Domain.Entities;
using Library.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Library.Infrastructure.Persistence.Seed;

public static class LibrarySeeder
{
    public static async Task SeedAsync(
        LibraryDbContext db,
        int authorCount = 50,
        int categoryCount = 10,
        bool forceReset = false
    )
    {
        if (forceReset)
        {
            await ClearAsync(db);
        }
        else if (await db.Authors.AnyAsync())
        {
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
            .CustomInstantiator(f => new Book(
                f.Commerce.ProductName(),
                f.PickRandom(authors),
                new Price(f.Random.Decimal(5, 50), "EUR")
            ));

        var books = bookFaker.Generate(authorCount * 10); // ~10 livres par auteur en moyenne

        foreach (var book in books)
        {
            var randomCategories = new Faker().PickRandom(categories, new Faker().Random.Int(1, 3));
            foreach (var category in randomCategories)
                book.AddCategory(category);
        }

        // Create fake data for company.
        var companyFaker = new Faker<Company>()
            .CustomInstantiator(f => new Company(f.Name.JobTitle()));
        var companies = companyFaker.Generate(10);

        db.Authors.AddRange(authors);
        db.Books.AddRange(books);
        db.Companies.AddRange(companies);

        await db.SaveChangesAsync();
    }

    public static async Task ClearAsync(LibraryDbContext db)
    {
        // Supprime directement la table de jointure via SQL brut, plus fiable pour une relation N-N
        await db.Database.ExecuteSqlRawAsync("DELETE FROM BookCategories");
        await db.Books.ExecuteDeleteAsync();
        await db.Authors.ExecuteDeleteAsync();
        await db.Categories.ExecuteDeleteAsync();
    }

    public static async Task CreateOpenIdDictAsync(IServiceScope scope, string clientId, string clientSecret)
    {
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await applicationManager.FindByClientIdAsync("playground-worker") is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "authors.read"
                }
            };

            await applicationManager.CreateAsync(descriptor);
        }
    }
}