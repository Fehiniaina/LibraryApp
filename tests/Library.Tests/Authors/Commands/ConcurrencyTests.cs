// tests/Library.Tests/Authors/Commands/ConcurrencyTests.cs
using FluentAssertions;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Library.Tests.TestHelpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Library.Tests.Authors.Commands;

[Collection("Database collection")]
public class ConcurrencyTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private IDbContextTransaction _transaction = default!;
    private LibraryDbContext _db = default!;
    private SqlConnection _connection = default!;

    public ConcurrencyTests(SqlServerContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _connection = new SqlConnection(_fixture.ConnectionString);
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(_connection)
            .Options;

        _db = new LibraryDbContext(options);
        await _db.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.RollbackTransactionAsync();
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task ConcurrentUpdate_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var author = new Author("Isaac", "Asimov");
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        // "B" partage la MÊME connexion physique — donc peut rejoindre la même transaction
        var optionsB = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(_connection)
            .Options;

        await using var dbB = new LibraryDbContext(optionsB);
        await dbB.Database.UseTransactionAsync(_db.Database.CurrentTransaction!.GetDbTransaction());

        var authorA = await _db.Authors.FirstAsync(a => a.Id == author.Id);
        var authorB = await dbB.Authors.FirstAsync(a => a.Id == author.Id);

        // Act
        authorB.UpdateName("ModifiéParB", authorB.LastName);
        await dbB.SaveChangesAsync();

        authorA.UpdateName("ModifiéParA", authorA.LastName);
        var act = async () => await _db.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}