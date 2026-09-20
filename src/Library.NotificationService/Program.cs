// src/Library.NotificationService/Program.cs
using Grpc.Net.Client;

using Library.Api.Grpc;
using Library.Domain.Events.Customers;
using Library.NotificationService.Services;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IEmailSender, FakeEmailSender>();

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

        cfg.ReceiveEndpoint("customer-created-queue", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // 3 tentatives, 5s d'intervalle
            e.ConfigureConsumer<CustomerCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();

#pragma warning disable CA1812, CA1852
internal class CustomerCreatedConsumer : IConsumer<CustomerCreatedEvent>
{
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _sender;

    public CustomerCreatedConsumer(IConfiguration configuration, IEmailSender sender)
    {
        _configuration = configuration;
        _sender = sender;
    }

    public async Task Consume(ConsumeContext<CustomerCreatedEvent> context)
    {
        var grpcUri = _configuration["grpcUri"] ?? throw new InvalidOperationException("grpcUri non configuré.");
        using var channel = GrpcChannel.ForAddress(grpcUri);
        var client = new CustomerGrpc.CustomerGrpcClient(channel);

        var reply = await client.GetCustomerDetailsAsync(new CustomerRequest
        {
            CustomerId = context.Message.CustomerId.ToString()
        });

        if (reply.Found)
        {
            await _sender.SendWelcomeEmailAsync(context.Message.Name, context.CancellationToken);
            Console.WriteLine($">>> [NotificationService] Bienvenue {reply.Name} de l'entreprise {reply.CompanyName} !");
        }
    }
}

#pragma warning restore CA1812, CA1852