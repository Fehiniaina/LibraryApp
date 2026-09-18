namespace Library.Domain.Entities;

using Library.Domain.ValueObjects;

public class Book
{
    private readonly List<Category> _categories = new ();

    public Book(string title, Author author, Price price)
    {
        this.Id = Guid.NewGuid();
        this.Title = title ?? throw new ArgumentNullException(nameof(title));
        this.Author = author ?? throw new ArgumentNullException(nameof(author));
        this.AuthorId = author.Id;
        this.Price = price ?? throw new ArgumentNullException(nameof(price));
    }

    // Pour lazy loading : la constructeur doit etre protected pas private
    private Book()
    {
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = default!;

    public Guid AuthorId { get; private set; }

    // public virtual Author Author => lazy loading
    public Author Author { get; private set; } = default!;

    public Price Price { get; private set; } = default!;

    // public virtual IReadOnlyCollection<Category> Categories => lazy loading
    public IReadOnlyCollection<Category> Categories => this._categories.AsReadOnly();

    public void AddCategory(Category category)
    {
        if (category is null)
        {
            ArgumentNullException.ThrowIfNull(category);
        }

        if (!this._categories.Contains(category))
        {
            this._categories.Add(category);
        }
    }

    public void UpdatePrice(Price newPrice) => this.Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
}