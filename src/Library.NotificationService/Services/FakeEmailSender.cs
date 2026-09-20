// src/Library.NotificationService/Services/FakeEmailSender.cs
using Microsoft.Extensions.Configuration;

namespace Library.NotificationService.Services;

#pragma warning disable CA1812 
internal sealed class FakeEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public FakeEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendWelcomeEmailAsync(string customerName, CancellationToken ct)
    {
        var shouldFail = _configuration.GetValue<bool>("SimulateEmailFailure");

        if (shouldFail)
        {
            Console.WriteLine($">>> Simulation d'échec d'envoi d'email pour {customerName} !");
            throw new InvalidOperationException("Simulation : service SendGrid indisponible.");
        }

        await Task.Delay(200, ct);
        Console.WriteLine($">>> [Email] Bienvenue {customerName} ! (email simulé envoyé)");
        // AUCUNE autre ligne Console.WriteLine ici — supprime "Email de bienvenue envoyé à {{CustomerName}} !"
    }
}
#pragma warning restore CA1812