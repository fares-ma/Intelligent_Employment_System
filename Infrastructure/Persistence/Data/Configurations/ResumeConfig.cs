using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class ResumeConfig : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Candidate)
            .WithMany(c => c.Resumes)
            .HasForeignKey(r => r.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.StoredFilePath)
            .IsRequired();

        builder.Property(r => r.FileType)
            .HasMaxLength(20);

        builder.Property(r => r.IsDefault)
            .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("getutcdate()");

        builder.HasIndex(r => r.CandidateId);
        builder.HasIndex(r => new { r.CandidateId, r.IsDefault });
    }
}
