// src/Library.Infrastructure/Jobs/RefreshTokenCleanupJob.cs
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Library.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class RefreshTokenCleanupJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RefreshTokenCleanupJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        var deletedCount = await db.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked)
            .ExecuteDeleteAsync(cancellationToken); // Bulk Operation — pas de chargement en mémoire
    }
}