using MediatR;

using Microsoft.Extensions.Logging;

namespace Library.Application.Customers.EventHandlers;

/// <summary>
/// Documentation for ResilientNotificationPublisher.
/// </summary>
public partial class ResilientNotificationPublisher : INotificationPublisher
{
    private readonly ILogger<ResilientNotificationPublisher> _logger;

    public ResilientNotificationPublisher(ILogger<ResilientNotificationPublisher> logger) => _logger = logger;

    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handler in handlerExecutors)
        {
            try
            {
                await handler.HandlerCallback(notification, cancellationToken);
            }
            catch (Exception)
            {
                LoggerMessage(notification.GetType().Name);
            }
        }
    }

    public void LoggerMessage(string NotificationType)
    {
        LogException(NotificationType);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Erreur dans un Notification Handler pour {NotificationType}.")]
    public partial void LogException(string NotificationType);
}
