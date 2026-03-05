using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class RecruiterConfig : IEntityTypeConfiguration<Recruiter>
{
    public void Configure(EntityTypeBuilder<Recruiter> builder)
    {
        // Recruiter → Company FK (Restrict to prevent cascade chain into Identity tables)
        builder.HasOne(r => r.Company)
            .WithMany(c => c.Recruiters)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.RecruiterRole)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(r => r.CompanyId);
    }
}
