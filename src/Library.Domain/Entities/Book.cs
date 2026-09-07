namespace Library.Domain.Entities;

public class Book
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = default!;
    public Guid AuthorId { get; private set; }

    // public virtual Author Author => lazy loading
    public Author Author { get; private set; } = default!;

    private readonly List<Category> _categories = new();

    // public virtual IReadOnlyCollection<Category> Categories => lazy loading
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    // Pour lazy loading : la constructeur doit etre protected pas private
    private Book() { }

    public Book(string title, Author author)
    {
        Id = Guid.NewGuid();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Author = author ?? throw new ArgumentNullException(nameof(author));
        AuthorId = author.Id;
    }

    public void AddCategory(Category category)
    {
        if (category is null) throw new ArgumentNullException(nameof(category));
        if (!_categories.Contains(category))
            _categories.Add(category);
    }
}