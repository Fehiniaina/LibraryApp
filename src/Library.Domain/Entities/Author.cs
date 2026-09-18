namespace Library.Domain.Entities;

using Library.Domain.Interfaces;

public class Author : IAuditableEntity
{
    private readonly List<Book> _books = new ();

    public Author(string firstName, string lastName)
    {
        this.Id = Guid.NewGuid();
        this.FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        this.LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
    }

    private Author()
    {
    }

    // ─── Propriétés publiques ─────────────────────────────────────────────────
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = default!;

    public string LastName { get; private set; } = default!;

    public byte[] RowVersion { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Gets Collection des livres associés à cet auteur.
    /// </summary>
    public IReadOnlyCollection<Book> Books => this._books.AsReadOnly();

    /// <inheritdoc/>
    public void SetCreatedAt(DateTime dateTime) => this.CreatedAt = dateTime;

    /// <inheritdoc/>
    public void SetUpdatedAt(DateTime dateTime) => this.UpdatedAt = dateTime;

    /// <summary>
    /// Met à jour le prénom et le nom de l'auteur.
    /// </summary>
    /// <param name="firstName">Nouveau prénom.</param>
    /// <param name="lastName">Nouveau nom.</param>
    /// <exception cref="ArgumentNullException">Levée si l'un des paramètres est null.</exception>
    public void UpdateName(string firstName, string lastName)
    {
        this.FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        this.LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
    }
}