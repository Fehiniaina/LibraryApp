using Library.Domain.Events.Customers;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Library.Application.Customers.EventHandlers;

/// <summary>
/// Documentation for LogAuditTrailHandler.
/// </summary>
public partial class LogAuditTrailHandler : IConsumer<CustomerCreatedEvent>
{
    private readonly ILogger<LogAuditTrailHandler> _logger;

    public LogAuditTrailHandler(ILogger<LogAuditTrailHandler> logger) => _logger = logger;

    public Task Consume(ConsumeContext<CustomerCreatedEvent> context)
    {
        LogTaskCompleted();
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = ">>> [AUDIT] Customer créé.")]
    private partial void LogTaskCompleted();
}
