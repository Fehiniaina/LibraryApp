namespace Playground.Mvc.Clients;

public interface ILibraryApiSyncClient
{
    Task<int> GetAuthorCountAsync(CancellationToken cancellationToken);
}
