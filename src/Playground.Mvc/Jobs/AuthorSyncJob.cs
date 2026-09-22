namespace Playground.Mvc.Jobs;

using Playground.Mvc.Clients;

using Quartz;

/// <summary>
/// Documentation for AuthorSyncJob.
/// </summary>
public partial class AuthorSyncJob : IJob
{
    private readonly ILibraryApiSyncClient _client;
    private readonly ILogger<AuthorSyncJob> _logger;

    public AuthorSyncJob(ILibraryApiSyncClient client, ILogger<AuthorSyncJob> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var count = await _client.GetAuthorCountAsync(context.CancellationToken);
        OnLogMessage(count, DateTime.UtcNow);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = ">> [Quartz Job] {Count} auteurs synchronisés à {Time}")]
    public partial void OnLogMessage(int Count, DateTime Time);
}
