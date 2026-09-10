using FluentAssertions;
using Library.Application.Customers.Commands.CreateCustomer;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.ValueObjects;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Repositories;
using Library.Infrastructure.Services;
using Library.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests.Customers.Commands
{
    [Collection("Database collection")]
    public class CreateCustomerCommandTests : IAsyncLifetime
    {
        private readonly SqlServerContainerFixture _fixture;
        private LibraryDbContext _db = default!;

        public CreateCustomerCommandTests(SqlServerContainerFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task DisposeAsync()
        {
            _db.Customers.RemoveRange(_db.Customers);
            _db.Companies.RemoveRange(_db.Companies);
            await _db.SaveChangesAsync();
            await _db.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            _db = new LibraryDbContext(options);
        }

        [Fact]
        public async Task Handle_WithValid_Company_CreateCustomer()
        {
            // Arrange
            var company = new Company("Acme Corp");

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            var customerRepository = new CustomerRepository(_db);
            var companyRepository = new Repository<Company>(_db);
            var customerService = new CustomerService(customerRepository, companyRepository);

            var handler = new CreateCustomerCommandHandler(_db, customerService);
            var command = new CreateCustomerCommand("Customer 1", company.Id, new Money(1000, "EUR"));

            var result = await handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.CustomerId.Should().NotBeNull();

            var customerExists = await _db.Customers.AnyAsync(c => c.Id == result.CustomerId);
            customerExists.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithInvalidCompany_ThrowsCompanyNotFoundException()
        {
            var nonExistentCompanyId = Guid.NewGuid();

            var customerRepository = new CustomerRepository(_db);
            var companyRepository = new Repository<Company>(_db);
            var customerService = new CustomerService(customerRepository, companyRepository);

            var handler = new CreateCustomerCommandHandler(_db, customerService);
            var command = new CreateCustomerCommand("Customer 1", nonExistentCompanyId, new Money(1000, "EUR"));

            var result = async() => await handler.Handle(command, CancellationToken.None);

            await result.Should().ThrowAsync<CompanyNotFoundException>();
        }
    }
}
