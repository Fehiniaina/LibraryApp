namespace Library.Domain.Entities
{
    public class Company
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;

        private readonly List<Customer> _customers = new();
        public virtual IReadOnlyCollection<Customer> Customers => _customers.AsReadOnly();

        protected Company() { }

        public Company(string name)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
