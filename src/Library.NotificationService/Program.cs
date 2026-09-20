// src/Library.NotificationService/Program.cs
using MassTransit;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CustomerCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();

#pragma warning disable CA1812, CA1852

// Les déclarations de types viennent APRÈS tous les top-level statements
internal record CustomerCreatedEvent(Guid CustomerId, string Name, Guid CompanyId);

internal class CustomerCreatedConsumer : IConsumer<CustomerCreatedEvent>
{
    public Task Consume(ConsumeContext<CustomerCreatedEvent> context)
    {
        Console.WriteLine($">>> [NotificationService] Reçu : Customer '{context.Message.Name}' créé !");
        return Task.CompletedTask;
    }
}

#pragma warning restore CA1812, CA1852