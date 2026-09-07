namespace Library.Domain.Entities;

public class Author
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;

    private readonly List<Book> _books = new();
    // Pour lazy loading : la propriété Books doit être virtual
    public IReadOnlyCollection<Book> Books => _books.AsReadOnly();

    public byte[] RowVersion { get; private set; } = default!;

    // Pour lazy loading : la constructeur doit etre protected pas private
    private Author() { } // pour EF Core

    public Author(string firstName, string lastName)
    {
        Id = Guid.NewGuid();
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
    }
}