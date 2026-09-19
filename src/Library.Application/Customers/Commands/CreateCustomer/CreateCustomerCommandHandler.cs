namespace Library.Application.Customers.Commands.CreateCustomer;

using Library.Domain.Entities;
using Library.Domain.Events.Customers;
using Library.Domain.Interfaces.Services;
using Library.Infrastructure.Persistence;

using MediatR;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResult>
{
    private readonly LibraryDbContext _db;

    private readonly ICustomerService _customerService;

    private readonly IPublisher _publisher;

    public CreateCustomerCommandHandler(LibraryDbContext db, ICustomerService customerService, IPublisher publisher)
    {
        _db = db;
        _customerService = customerService;
        _publisher = publisher;
    }

    public async Task<CreateCustomerResult> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        Customer customer;

        try
        {
            customer = await _customerService.CreateCustomerAsync(
                    request.Name,
                    request.CompanyId,
                    request.CreditLimit,
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        // Publish event
        await _publisher.Publish(
            new CustomerCreatedEvent(customer.Id, customer.Name, customer.Company.Id),
            cancellationToken);

        return new CreateCustomerResult(true, customer.Id, null);
    }
}
