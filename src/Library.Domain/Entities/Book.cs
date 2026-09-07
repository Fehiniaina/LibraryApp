using Library.Domain.ValueObjects;

namespace Library.Domain.Entities;

public class Book
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = default!;
    public Guid AuthorId { get; private set; }

    // public virtual Author Author => lazy loading
    public Author Author { get; private set; } = default!;
    public Price Price { get; private set; } = default!;

    private readonly List<Category> _categories = new();

    // public virtual IReadOnlyCollection<Category> Categories => lazy loading
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    // Pour lazy loading : la constructeur doit etre protected pas private
    public Book() { }

    public Book(string title, Author author, Price price)
    {
        Id = Guid.NewGuid();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Author = author ?? throw new ArgumentNullException(nameof(author));
        AuthorId = author.Id;
        Price = price ?? throw new ArgumentNullException(nameof(price));
    }

    public void AddCategory(Category category)
    {
        if (category is null) throw new ArgumentNullException(nameof(category));
        if (!_categories.Contains(category))
            _categories.Add(category);
    }

    public void UpdatePrice(Price newPrice) => Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
}