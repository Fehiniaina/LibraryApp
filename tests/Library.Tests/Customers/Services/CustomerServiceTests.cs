using FluentAssertions;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;
using Library.Domain.ValueObjects;
using Library.Infrastructure.Services;
using Moq;
using Xunit;

namespace Library.Tests.Customers.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
        private readonly Mock<IRepository<Company>> _companyRepositoryMock = new();
        private readonly CustomerService _sut; // "sut" => System Under Test

        public CustomerServiceTests()
        {
            _sut = new CustomerService(_customerRepositoryMock.Object, _companyRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateCustomerAsync_WithInvalidCompany_CreateCustomer()
        {
            // Toujours le principes AAA : Arrange, Act and Assert
            // Arrange
            var company = new Company("Acme Corp");
            var creditLimit = new Money(1000, "EUR");

            _companyRepositoryMock
                .Setup(r => r.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(company);
            // Act
            var result = await _sut.CreateCustomerAsync("Jean Dupont", company.Id, creditLimit, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Jean Dupont");
            result.CreditLimit.Amount.Should().Be(1000);

            _customerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
            _customerRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAsync_WithNonExistentCompany_ThrowsCompanyNotFoundException()
        {
            // Arrange
            var company = new Company("Acme Corp");
            var creditLimit = new Money(1000, "EUR");

            // Return null
            _companyRepositoryMock
                .Setup(r => r.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Company?)null);

            // Act
            var result = async() => await _sut.CreateCustomerAsync("Jean Dupont", company.Id, creditLimit, CancellationToken.None);

            // Assert
            await result.Should().ThrowAsync<CompanyNotFoundException>();

            _customerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
            _customerRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        }
    }
}
