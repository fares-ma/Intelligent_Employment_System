using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class InterviewConfig : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.JobApplication)
            .WithMany(ja => ja.Interviews)
            .HasForeignKey(i => i.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.InterviewType)
            .HasConversion<int>();

        builder.Property(i => i.Status)
            .HasConversion<int>();

        builder.Property(i => i.DurationMinutes)
            .HasDefaultValue(60);

        builder.Property(i => i.Score)
            .HasPrecision(5, 2);

        builder.Property(i => i.ScheduledAt)
            .IsRequired();

        builder.HasIndex(i => i.JobApplicationId);
        builder.HasIndex(i => i.ScheduledAt);
        builder.HasIndex(i => i.Status);
    }
}
