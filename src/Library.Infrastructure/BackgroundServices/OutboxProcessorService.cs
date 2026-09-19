using System.Text.Json;

using Library.Domain.Events.Customers;
using Library.Infrastructure.Persistence;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.BackgroundServices;

public partial class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;

    public OutboxProcessorService(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10)); // MOVE TO settings

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                LogOutboxProcessingError(ex);
            }
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var pendingMessages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(20)
            .ToListAsync(ct);

        foreach (var message in pendingMessages)
        {
            try
            {
                var domainEvent = DeserializeEvent(message.Type, message.Content);
                if (domainEvent is not null)
                {
                    await publisher.Publish(domainEvent, ct);
                }

                message.MarkAsProcessed();
                LogPublishedWithSuccess(message.Id, message.Type);
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);
                LogPublishedFailure(message.Id, ex);
            }
        }

        await db.SaveChangesAsync(ct);
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static INotification? DeserializeEvent(string type, string content) => type switch
    {
        nameof(CustomerCreatedEvent) => JsonSerializer.Deserialize<CustomerCreatedEvent>(content),
        _ => null // type d'événement inconnu — ignoré, pourrait aussi logger un warning
    };
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

    [LoggerMessage(Level = LogLevel.Error, Message = "Erreur lors du traitement de l'Outbox")]
    private partial void LogOutboxProcessingError(Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "OutboxMessage {Id} ({Type}) publié avec succès")]
    private partial void LogPublishedWithSuccess(Guid id, string type);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Échec de publication du OutboxMessage {Id}")]
    private partial void LogPublishedFailure(Guid id, Exception ex);
}