using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;
using Library.Domain.Interfaces.Services;
using Library.Domain.ValueObjects;

namespace Library.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepository<Company> _companyRepository;
        public CustomerService(
            ICustomerRepository customerRepository,
            IRepository<Company> companyRepository
        )
        {
            _customerRepository = customerRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Customer> CreateCustomerAsync(string name, Guid companyId, Money creditLimit, CancellationToken ct)
        {
            var company = await _companyRepository.GetByIdAsync(companyId, ct);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            var customer = new Customer(name, company, creditLimit);
            await _customerRepository.AddAsync(customer, ct);
            await _customerRepository.SaveChangesAsync(ct);

            return customer;
        }
    }
}
