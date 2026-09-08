// src/Library.Domain/Interfaces/IAuditableEntity.cs
namespace Library.Domain.Interfaces;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    void SetCreatedAt(DateTime dateTime);
    void SetUpdatedAt(DateTime dateTime);
}