namespace Library.Application.Customers.Commands.CreateCustomer;

using System.Text.Json;

using Library.Domain.Entities;
using Library.Domain.Events.Customers;
using Library.Domain.Interfaces.Services;
using Library.Infrastructure.Persistence;

using MediatR;

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
        Customer customer;

        try
        {
            customer = await _customerService.CreateCustomerAsync(
                    request.Name,
                    request.CompanyId,
                    request.CreditLimit,
                    cancellationToken);

            var domainEvent = new CustomerCreatedEvent(customer.Id, customer.Name, customer.Company.Id);
            var outboxMessage = new OutboxMessage(
                type: nameof(CustomerCreatedEvent),
                content: JsonSerializer.Serialize(domainEvent));

            _db.OutboxMessages.Add(outboxMessage);

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken); // atomique — les DEUX ou AUCUN
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new CreateCustomerResult(true, customer.Id, null);
    }
}
