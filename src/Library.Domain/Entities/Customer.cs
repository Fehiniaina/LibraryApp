namespace Library.Domain.Entities;

using Library.Domain.ValueObjects;

public class Customer
{
    public Customer(string name, Company company, Money creditLimit)
    {
        this.Id = Guid.NewGuid();
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
        this.Company = company ?? throw new ArgumentNullException(nameof(company));
        this.CompanyId = company.Id;
        this.CreditLimit = creditLimit ?? throw new ArgumentNullException(nameof(creditLimit));
    }

    private Customer()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public Guid CompanyId { get; private set; }

    public virtual Company Company { get; private set; } = default!;

    public Money CreditLimit { get; private set; } = default!;
}
