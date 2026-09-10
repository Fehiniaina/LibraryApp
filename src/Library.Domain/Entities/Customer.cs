using Library.Domain.ValueObjects;

namespace Library.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public Guid CompanyId { get; private set; }
        public virtual Company Company { get; private set; } = default!;
        public Money CreditLimit { get; private set; } = default!;

        protected Customer() { }

        public Customer(string name, Company company, Money creditLimit)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Company = company ?? throw new ArgumentNullException(nameof(company));
            CompanyId = company.Id;
            CreditLimit = creditLimit ?? throw new ArgumentNullException(nameof(creditLimit));
        }
    }
}
