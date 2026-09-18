namespace Library.Application.Customers.Commands.CreateCustomer;

using Library.Domain.Interfaces.Services;
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResult>
{
    private readonly LibraryDbContext _db;
    private readonly ICustomerService _customerService;

    public CreateCustomerCommandHandler(LibraryDbContext db, ICustomerService customerService)
    {
        _db = db;
        _customerService = customerService;
    }

    public async Task<CreateCustomerResult> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await _customerService.CreateCustomerAsync(
                    request.Name,
                    request.CompanyId,
                    request.CreditLimit,
                    cancellationToken
            );

            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return new CreateCustomerResult(true, customer.Id, null);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
