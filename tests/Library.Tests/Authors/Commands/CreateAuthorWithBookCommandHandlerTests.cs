// tests/Library.Tests/Authors/Commands/CreateAuthorWithBookCommandHandlerTests.cs
using Library.Application.Authors.Commands.CreateAuthorWithBook;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Library.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Library.Tests.Authors.Commands;

[Collection("Database collection")]
public class CreateAuthorWithBookCommandHandlerTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private LibraryDbContext _db = default!;

    public CreateAuthorWithBookCommandHandlerTests(SqlServerContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _db = new LibraryDbContext(options);
        // PAS de transaction ouverte ici — le Handler gère la sienne en interne
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Nettoyage manuel après chaque test, puisqu'on ne peut pas compter sur un rollback englobant
        _db.Books.RemoveRange(_db.Books);
        _db.Authors.RemoveRange(_db.Authors);
        await _db.SaveChangesAsync();
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Handle_WithDuplicateTitle_RollsBackAuthorCreation()
    {
        var existingAuthor = new Author("Existing", "Author");
        _db.Authors.Add(existingAuthor);
        _db.Books.Add(new Book("Titre Unique Test", existingAuthor, new Library.Domain.ValueObjects.Price(10, "EUR")));
        await _db.SaveChangesAsync();

        var handler = new CreateAuthorWithBookCommandHandler(_db);
        var command = new CreateAuthorWithBookCommand("Nouvel", "Auteur", "Titre Unique Test", 15, "EUR");

        var result = await handler.Handle(command, CancellationToken.None);
         
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("existe déjà");

        var authorExists = await _db.Authors.AnyAsync(a => a.FirstName == "Nouvel" && a.LastName == "Auteur");
        authorExists.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesAuthorAndBook()
    {
        var handler = new CreateAuthorWithBookCommandHandler(_db);
        var command = new CreateAuthorWithBookCommand("Jean", "Valjean", "Les Misérables Test", 25, "EUR");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AuthorId.Should().NotBeNull();

        var bookExists = await _db.Books.AnyAsync(b => b.Title == "Les Misérables Test");
        bookExists.Should().BeTrue();
    }
}