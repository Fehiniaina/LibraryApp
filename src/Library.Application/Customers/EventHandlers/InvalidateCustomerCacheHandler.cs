// src/Library.Application/Customers/EventHandlers/InvalidateCustomerCacheHandler.cs
using Library.Domain.Events.Customers;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Library.Application.Customers.EventHandlers;

/// <summary>
/// Documentation for InvalidateCustomerCacheHandler.
/// </summary>
public partial class InvalidateCustomerCacheHandler : IConsumer<CustomerCreatedEvent>
{
    private readonly ILogger<InvalidateCustomerCacheHandler> _logger;

    public InvalidateCustomerCacheHandler(ILogger<InvalidateCustomerCacheHandler> logger) => _logger = logger;

    public Task Consume(ConsumeContext<CustomerCreatedEvent> context)
    {
        LogCacheInvalidated();
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Invalidate cache")]
    private partial void LogCacheInvalidated();
}