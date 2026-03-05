using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class CompanyConfig : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Industry)
            .HasMaxLength(100);

        builder.Property(c => c.Website)
            .HasMaxLength(500);

        builder.Property(c => c.TaxNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.TaxNumber)
            .IsUnique();

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.LogoPath)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .IsRequired();
    }
}
