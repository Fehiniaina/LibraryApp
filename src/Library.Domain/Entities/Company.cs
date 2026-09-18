namespace Library.Domain.Entities;

public class Company
{
    private readonly List<Customer> _customers = new ();

    public Company(string name)
    {
        this.Id = Guid.NewGuid();
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    private Company()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public virtual IReadOnlyCollection<Customer> Customers => this._customers.AsReadOnly();
}
