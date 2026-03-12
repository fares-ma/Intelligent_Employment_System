using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class CandidateSkillConfig : IEntityTypeConfiguration<CandidateSkill>
{
    public void Configure(EntityTypeBuilder<CandidateSkill> builder)
    {
        builder.HasKey(cs => new { cs.CandidateId, cs.SkillId });

        builder.HasOne(cs => cs.Candidate)
            .WithMany(c => c.CandidateSkills)
            .HasForeignKey(cs => cs.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Skill)
            .WithMany(s => s.CandidateSkills)
            .HasForeignKey(cs => cs.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(cs => cs.Level)
            .HasConversion<int>();
    }
}
