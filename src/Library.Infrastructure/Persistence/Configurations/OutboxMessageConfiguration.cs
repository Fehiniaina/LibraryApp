using Library.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Content).IsRequired(); // nvarchar(max) par défaut, suffisant pour du JSON

        builder.HasIndex(m => m.ProcessedAt); // le BackgroundService va filtrer "WHERE ProcessedAt IS NULL" en boucle
    }
}