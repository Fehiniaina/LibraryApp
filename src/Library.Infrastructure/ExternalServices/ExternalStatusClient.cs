// src/Library.Infrastructure/ExternalServices/ExternalStatusClient.cs
using Library.Domain.Interfaces;

namespace Library.Infrastructure.ExternalServices;

public class ExternalStatusClient : IExternalStatusClient
{
    private readonly HttpClient _httpClient;

    public ExternalStatusClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<bool> CheckStatusAsync(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("500", ct); // 50% échec volontaire
        return response.IsSuccessStatusCode;
    }
}