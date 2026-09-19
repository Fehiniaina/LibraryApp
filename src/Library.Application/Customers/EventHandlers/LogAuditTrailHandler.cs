using Library.Domain.Events.Customers;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Library.Application.Customers.EventHandlers;

/// <summary>
/// Documentation for LogAuditTrailHandler.
/// </summary>
public partial class LogAuditTrailHandler : INotificationHandler<CustomerCreatedEvent>
{
    private readonly ILogger<LogAuditTrailHandler> _logger;

    public LogAuditTrailHandler(ILogger<LogAuditTrailHandler> logger) => _logger = logger;

    public Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        LoggerMessage(notification.CustomerId.ToString());

        return Task.CompletedTask;
    }

    public void LoggerMessage(string CustomerId)
    {
        LogAuditTrailer(CustomerId);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = ">>> [AUDIT] Customer {CustomerId} créé.")]
    private partial void LogAuditTrailer(string CustomerId);
}
