// src/Library.Infrastructure/ExternalServices/LoggingDelegatingHandler.cs
namespace Library.Infrastructure.ExternalServices;

public class LoggingDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        sw.Stop();

        return response;
    }
}