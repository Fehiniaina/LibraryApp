using Library.Application.Authors.Commands.CreateAuthorWithBook;
using Library.Application.Customers.Exceptions;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResult>
    {
        private readonly LibraryDbContext _db;
        public CreateCustomerCommandHandler(LibraryDbContext db) => _db = db;

        public async Task<CreateCustomerResult> Handle(CreateCustomerCommand request, CancellationToken ct)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                // Etape 1 : On verifie l'existence du company.
                var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, ct);
                if (null == company) throw new CompanyNotFoundException(request.CompanyId);

                var customer = new Customer(request.Name, company, request.CreditLimit);
                _db.Add(customer);
                await _db.SaveChangesAsync(ct);

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
