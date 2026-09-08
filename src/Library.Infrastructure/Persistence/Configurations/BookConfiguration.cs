using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
        builder.HasMany(b => b.Categories)
           .WithMany(b => b.Books)
           .UsingEntity(j => j.ToTable("BookCategories"));

        builder.ComplexProperty(b => b.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("PriceAmount").HasColumnType("decimal(10,2)");
            price.Property(p => p.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
        });
    }
}