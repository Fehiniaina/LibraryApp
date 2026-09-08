// tests/Library.Tests/Authors/Queries/SearchAuthorsQueryHandlerIntegrationTests.cs
using Library.Application.Authors.Queries.SearchAuthors;
using Library.Domain.Entities;
using Library.Domain.ValueObjects;
using Library.Infrastructure.Persistence;
using Library.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace Library.Tests.Authors.Queries;

[Collection("Database collection")]
public class SearchAuthorsQueryHandlerIntegrationTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private LibraryDbContext _db = default!;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction _transaction = default!;

    public SearchAuthorsQueryHandlerIntegrationTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _db = new LibraryDbContext(options);
        _transaction = await _db.Database.BeginTransactionAsync(); // applique tes VRAIES migrations sur le conteneur
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Handle_WithMinBookCountFilter_ExcludesAuthorsWithFewerBooks()
    {
        // Arrange
        var authorWithBooks = new Author("Victor", "Hugo");
        var authorWithoutBooks = new Author("Jules", "Verne");
        _db.Authors.AddRange(authorWithBooks, authorWithoutBooks);

        _db.Books.Add(new Book("Les Misérables", authorWithBooks, new Price(20, "EUR")));
        await _db.SaveChangesAsync(); // RowVersion généré NATIVEMENT par le vrai SQL Server — aucun contournement nécessaire !

        var handler = new SearchAuthorsQueryHandler(_db);
        var query = new SearchAuthorsQuery(LastName: null, MinBookCount: 1, Page: 1, PageSize: 20);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Single().LastName.Should().Be("Hugo");
    }

    [Fact]
    public async Task Handle_WithLastNameFilter_IsCaseInsensitive()
    {
        // Arrange — teste EXACTEMENT le comportement qu'InMemory ne pouvait pas valider
        _db.Authors.Add(new Author("Victor", "HUGO"));
        await _db.SaveChangesAsync();

        var handler = new SearchAuthorsQueryHandler(_db);
        var query = new SearchAuthorsQuery(LastName: "hugo", MinBookCount: null, Page: 1, PageSize: 20); // minuscule

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — grâce à la collation Latin1_General_CI_AS (Case Insensitive), ça doit matcher
        result.TotalCount.Should().Be(1);
    }
}