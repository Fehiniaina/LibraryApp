// src/Library.NotificationService/Services/IEmailSender.cs
namespace Library.NotificationService.Services;

internal interface IEmailSender
{
    Task SendWelcomeEmailAsync(string customerName, CancellationToken ct);
}