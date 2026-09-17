namespace Library.Domain.Entities;

public class Category : IEquatable<Category>
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;

    private readonly List<Book> _books = new();
    // public virtual => Pour lazy loading
    public IReadOnlyCollection<Book> Books => _books.AsReadOnly(); // <-- ajouté

    // Pour lazy loading : la constructeur doit etre protected pas private
    protected Category() { }

    public Category(string name)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public bool Equals(Category? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => Equals(obj as Category);
    public override int GetHashCode() => Id.GetHashCode();
}