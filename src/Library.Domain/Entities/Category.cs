namespace Library.Domain.Entities;

public class Category : IEquatable<Category>
{
    private readonly List<Book> books = new ();

    public Category(string name)
    {
        this.Id = Guid.NewGuid();
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    protected Category()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    // public virtual => Pour lazy loading
    public IReadOnlyCollection<Book> Books => this.books.AsReadOnly(); // <-- ajouté

    public bool Equals(Category? other) => other is not null && this.Id == other.Id;

    public override bool Equals(object? obj) => this.Equals(obj as Category);

    public override int GetHashCode() => this.Id.GetHashCode();
}