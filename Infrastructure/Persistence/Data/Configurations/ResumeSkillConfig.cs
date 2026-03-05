using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class ResumeSkillConfig : IEntityTypeConfiguration<ResumeSkill>
{
    public void Configure(EntityTypeBuilder<ResumeSkill> builder)
    {
        builder.HasKey(rs => new { rs.ResumeId, rs.SkillId });

        builder.HasOne(rs => rs.Resume)
            .WithMany(r => r.ResumeSkills)
            .HasForeignKey(rs => rs.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.Skill)
            .WithMany(s => s.ResumeSkills)
            .HasForeignKey(rs => rs.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
