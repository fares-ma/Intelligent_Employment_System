using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class SavedJobConfig : IEntityTypeConfiguration<SavedJob>
{
    public void Configure(EntityTypeBuilder<SavedJob> builder)
    {
        builder.HasKey(sj => new { sj.CandidateId, sj.JobPostId });

        builder.HasOne(sj => sj.Candidate)
            .WithMany(c => c.SavedJobs)
            .HasForeignKey(sj => sj.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sj => sj.JobPost)
            .WithMany()
            .HasForeignKey(sj => sj.JobPostId)
            .OnDelete(DeleteBehavior.NoAction);  // Prevent multiple cascade paths from Company

        builder.Property(sj => sj.SavedAt)
            .HasDefaultValueSql("getutcdate()");

        builder.HasIndex(sj => sj.CandidateId);
        builder.HasIndex(sj => sj.JobPostId);
    }
}
