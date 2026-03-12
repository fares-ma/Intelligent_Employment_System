using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class CandidateEducationConfig : IEntityTypeConfiguration<CandidateEducation>
{
    public void Configure(EntityTypeBuilder<CandidateEducation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Candidate)
            .WithMany(c => c.CandidateEducations)
            .HasForeignKey(e => e.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.Degree)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FieldOfStudy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Institution)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("getutcdate()");

        builder.HasIndex(e => e.CandidateId);
    }
}
