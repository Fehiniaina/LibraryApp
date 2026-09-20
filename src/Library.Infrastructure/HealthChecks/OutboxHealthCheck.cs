using Library.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Library.Infrastructure.HealthChecks;

public class OutboxHealthCheck : IHealthCheck
{
    private readonly LibraryDbContext _db;

    public OutboxHealthCheck(LibraryDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var oldestPending = await _db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Select(m => m.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (oldestPending == default)
        {
            return HealthCheckResult.Healthy("Aucun message en attente dans l'Outbox.");
        }

        var delay = DateTime.UtcNow - oldestPending;

        if (delay > TimeSpan.FromMinutes(5))
        {
            return HealthCheckResult.Unhealthy($"Un message attend depuis {delay.TotalMinutes:F1} minutes — RabbitMQ probablement indisponible.");
        }

        return HealthCheckResult.Degraded($"Message en attente depuis {delay.TotalSeconds:F0}s — normal si < 10s.");
    }
}
