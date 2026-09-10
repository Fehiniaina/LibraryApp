using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(50);

            builder.ComplexProperty(b => b.CreditLimit, creditLimit =>
            {
                creditLimit.Property(p => p.Amount).HasColumnName("CreditLimitAmount").HasColumnType("decimal(10,2)");
                creditLimit.Property(p => p.Currency).HasColumnName("CreditLimitCurrency").HasMaxLength(3);
            });
        }
    }
}
