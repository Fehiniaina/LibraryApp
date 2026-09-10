using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(200);

            builder.HasMany(c => c.Customers)
               .WithOne(cu => cu.Company)
               .HasForeignKey(cu => cu.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
