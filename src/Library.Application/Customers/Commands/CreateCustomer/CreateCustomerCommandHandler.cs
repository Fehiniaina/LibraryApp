using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces.Services;
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResult>
    {
        private readonly LibraryDbContext _db;
        private readonly ICustomerService _customerService;

        public CreateCustomerCommandHandler(LibraryDbContext db, ICustomerService customerService)
        {
            _db = db;
            _customerService = customerService;
        }

        public async Task<CreateCustomerResult> Handle(CreateCustomerCommand request, CancellationToken ct)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var customer = await _customerService.CreateCustomerAsync(
                        request.Name,
                        request.CompanyId,
                        request.CreditLimit,
                        ct
                );

                var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, ct);

                await transaction.CommitAsync(ct);
                return new CreateCustomerResult(true, customer.Id, null);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
