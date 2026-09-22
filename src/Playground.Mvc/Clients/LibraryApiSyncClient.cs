namespace Playground.Mvc.Clients;

public class LibraryApiSyncClient : ILibraryApiSyncClient
{
    private readonly HttpClient _httpClient;

    public LibraryApiSyncClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> GetAuthorCountAsync(CancellationToken cancellationToken)
    {
        var authors = await _httpClient.GetFromJsonAsync<List<object>>("/authors", cancellationToken);

        return authors?.Count ?? 0;
    }
}
