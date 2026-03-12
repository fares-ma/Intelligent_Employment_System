using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class CandidateExperienceConfig : IEntityTypeConfiguration<CandidateExperience>
{
    public void Configure(EntityTypeBuilder<CandidateExperience> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Candidate)
            .WithMany(c => c.CandidateExperiences)
            .HasForeignKey(e => e.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.JobTitle)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Company)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("getutcdate()");

        builder.HasIndex(e => e.CandidateId);
    }
}
