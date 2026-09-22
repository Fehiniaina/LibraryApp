using System.Net.Http.Headers;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Playground.Mvc.Options;

namespace Playground.Mvc.Clients;

public class LibraryApiSyncClient : ILibraryApiSyncClient
{
    private const string TokenCacheKey = "oauth-client-credentials-token";
    private readonly HttpClient _httpClient;
    private readonly OAuthClientOptions _options;
    private readonly IMemoryCache _cache;

    public LibraryApiSyncClient(HttpClient httpClient, IOptions<OAuthClientOptions> options, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
    }

    public async Task<int> GetAuthorCountAsync(CancellationToken cancellationToken)
    {
        var token = await GetOrRefreshTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var result = await _httpClient.GetFromJsonAsync<CountResult>("/service/authors/count", cancellationToken);

        return result!.Count;
    }

    private async Task<string> GetOrRefreshTokenAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && cached is not null)
        {
            return cached;
        }

#pragma warning disable CA2234 // Pass system uri objects instead of strings
        var tokenResponse = await _httpClient.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["scope"] = "authors.read"
        }), ct);
#pragma warning restore CA2234 // Pass system uri objects instead of strings

        tokenResponse.EnsureSuccessStatusCode();
        var payload = await tokenResponse.Content.ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken: ct);

        _cache.Set(TokenCacheKey, payload!.AccessToken, TimeSpan.FromSeconds(payload.ExpiresIn - 60)); // marge de sécurité

        return payload.AccessToken;
    }

    private record CountResult(int Count);
    private record OAuthTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
