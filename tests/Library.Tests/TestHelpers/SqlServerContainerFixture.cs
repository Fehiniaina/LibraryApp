// tests/Library.Tests/TestHelpers/SqlServerContainerFixture.cs
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

namespace Library.Tests.TestHelpers;

public class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var db = new LibraryDbContext(options);
        await db.Database.MigrateAsync(); // UNE SEULE fois pour tout le conteneur
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}