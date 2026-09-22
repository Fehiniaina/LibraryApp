using Playground.Mvc.Clients;

namespace Playground.Mvc.Workers;

/// <summary>
/// Documentation for AuthorSyncWorker.
/// </summary>
public partial class AuthorSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuthorSyncWorker> _logger;

    public AuthorSyncWorker(IServiceScopeFactory scopeFactory, ILogger<AuthorSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<ILibraryApiSyncClient>();

                var count = await client.GetAuthorCountAsync(stoppingToken);
                OnLogMessage(count, DateTime.UtcNow);
            }
            catch (Exception e)
            {
                OnLogError(e);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[Worker/BackgroundService] {Count} auteurs synchronisés à {Time}")]
    public partial void OnLogMessage(int Count, DateTime Time);

    [LoggerMessage(Level = LogLevel.Error, Message = "Erreur de synchronisation (BackgroundService)")]
    public partial void OnLogError(Exception e);
}
