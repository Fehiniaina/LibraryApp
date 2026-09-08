// src/Library.Infrastructure/ExternalServices/LoggingDelegatingHandler.cs
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.ExternalServices;

public class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;
    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger) => _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        _logger.LogInformation(">>> [HTTP] Envoi : {Method} {Uri}", request.Method, request.RequestUri);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await base.SendAsync(request, ct);
        sw.Stop();

        _logger.LogInformation(">>> [HTTP] Reçu : {StatusCode} en {ElapsedMs}ms", response.StatusCode, sw.ElapsedMilliseconds);

        return response;
    }
}