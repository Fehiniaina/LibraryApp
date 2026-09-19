// src/Library.Application/Customers/EventHandlers/InvalidateCustomerCacheHandler.cs
using Library.Domain.Events.Customers;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Library.Application.Customers.EventHandlers;

/// <summary>
/// Documentation for InvalidateCustomerCacheHandler.
/// </summary>
public partial class InvalidateCustomerCacheHandler : INotificationHandler<CustomerCreatedEvent>
{
    private readonly ILogger<InvalidateCustomerCacheHandler> _logger;

    public InvalidateCustomerCacheHandler(ILogger<InvalidateCustomerCacheHandler> logger) => _logger = logger;

    public Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        LoggerMessage(notification.CompanyId.ToString(), notification.CustomerId.ToString());
        return Task.CompletedTask;
    }

    public void LoggerMessage(string CompanyId, string CustomerId)
    {
        LogCacheInvalidated(CompanyId, CustomerId);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache invalidé pour la company {CompanyId} suite à la création de {CustomerId}\"")]
    private partial void LogCacheInvalidated(string CompanyId, string CustomerId);
}