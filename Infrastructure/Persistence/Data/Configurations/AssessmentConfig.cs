using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class AssessmentConfig : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.JobPost)
            .WithMany(j => j.Assessments)
            .HasForeignKey(a => a.JobPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Questions)
            .WithOne(q => q.Assessment)
            .HasForeignKey(q => q.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Type)
            .HasConversion<int>();

        builder.Property(a => a.TimeLimitMinutes)
            .HasDefaultValue(60);

        builder.Property(a => a.Instructions)
            .IsRequired(false);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("getutcdate()");

        builder.HasIndex(a => a.JobPostId);
    }
}
