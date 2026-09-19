// src/Library.Domain/Events/CustomerCreatedEvent.cs
namespace Library.Domain.Events.Customers;

public record CustomerCreatedEvent(Guid CustomerId, string Name, Guid CompanyId);