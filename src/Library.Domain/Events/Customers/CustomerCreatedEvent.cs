// src/Library.Domain/Events/CustomerCreatedEvent.cs
namespace Library.Domain.Events.Customers;

using MediatR;

public record CustomerCreatedEvent(Guid CustomerId, string Name, Guid CompanyId) : INotification;