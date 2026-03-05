using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class JobApplicationConfig : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasKey(ja => ja.Id);

        // Foreign Keys
        builder.HasOne(ja => ja.Candidate)
            .WithMany(c => c.JobApplications)
            .HasForeignKey(ja => ja.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ja => ja.JobPost)
            .WithMany(j => j.JobApplications)
            .HasForeignKey(ja => ja.JobPostId)
            .OnDelete(DeleteBehavior.NoAction);  // Prevent multiple cascade paths from Company

        builder.HasOne(ja => ja.Resume)
            .WithMany(r => r.JobApplications)
            .HasForeignKey(ja => ja.ResumeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: (CandidateId, JobPostId) — prevent duplicate applications
        builder.HasIndex(ja => new { ja.CandidateId, ja.JobPostId })
            .IsUnique();

        // Indexes for filtering
        builder.HasIndex(ja => ja.CandidateId);
        builder.HasIndex(ja => ja.JobPostId);
        builder.HasIndex(ja => ja.Status);
        builder.HasIndex(ja => new { ja.JobPostId, ja.Status });

        // Field configuration
        builder.Property(ja => ja.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ja => ja.MatchScore)
            .HasPrecision(5, 2);

        builder.Property(ja => ja.AppliedAt)
            .HasDefaultValueSql("getutcdate()");
    }
}
