// src/Library.Infrastructure/Jobs/RefreshTokenCleanupJob.cs
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Library.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class RefreshTokenCleanupJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupJob> _logger;

    public RefreshTokenCleanupJob(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Démarrage du nettoyage des refresh tokens...");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        var deletedCount = await db.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked)
            .ExecuteDeleteAsync(cancellationToken); // Bulk Operation — pas de chargement en mémoire

        _logger.LogInformation("Nettoyage terminé — {Count} refresh tokens supprimés.", deletedCount);
    }
}