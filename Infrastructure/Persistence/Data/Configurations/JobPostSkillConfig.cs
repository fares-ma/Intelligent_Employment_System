using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class JobPostSkillConfig : IEntityTypeConfiguration<JobPostSkill>
{
    public void Configure(EntityTypeBuilder<JobPostSkill> builder)
    {
        builder.HasKey(jps => new { jps.JobPostId, jps.SkillId });

        // Properties
        builder.Property(jps => jps.IsRequired)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(jps => jps.RequiredLevel)
            .HasConversion<int>()
            .IsRequired(false);

        builder.HasOne(jps => jps.JobPost)
            .WithMany(j => j.JobPostSkills)
            .HasForeignKey(jps => jps.JobPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(jps => jps.Skill)
            .WithMany(s => s.JobPostSkills)
            .HasForeignKey(jps => jps.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
