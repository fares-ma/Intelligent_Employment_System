using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class CandidateAssessmentConfig : IEntityTypeConfiguration<CandidateAssessment>
{
    public void Configure(EntityTypeBuilder<CandidateAssessment> builder)
    {
        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.Answers)
            .HasMaxLength(5000);

        builder.Property(ca => ca.Score)
            .HasPrecision(5, 2);

        // ── Relationships (explicit to prevent EF from creating shadow properties) ──
        // Candidate → CandidateAssessment
        builder.HasOne(ca => ca.Candidate)
            .WithMany(c => c.CandidateAssessments)
            .HasForeignKey(ca => ca.CandidateId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_CandidateAssessment_Candidate");

        // Assessment → CandidateAssessment
        builder.HasOne(ca => ca.Assessment)
            .WithMany(a => a.CandidateAssessments)
            .HasForeignKey(ca => ca.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_CandidateAssessment_Assessment");

        // JobApplication → CandidateAssessment
        builder.HasOne(ca => ca.JobApplication)
            .WithMany(ja => ja.CandidateAssessments)
            .HasForeignKey(ca => ca.JobApplicationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_CandidateAssessment_JobApplication");

        // ── Constraints ──
        builder.HasIndex(ca => new { ca.CandidateId, ca.AssessmentId })
            .IsUnique()
            .HasDatabaseName("IX_CandidateAssessment_Candidate_Assessment_Unique");

        // ── Indexes ──
        builder.HasIndex(ca => ca.CandidateId)
            .HasDatabaseName("IX_CandidateAssessment_CandidateId");

        builder.HasIndex(ca => ca.AssessmentId)
            .HasDatabaseName("IX_CandidateAssessment_AssessmentId");

        builder.HasIndex(ca => ca.JobApplicationId)
            .HasDatabaseName("IX_CandidateAssessment_JobApplicationId");

        builder.HasIndex(ca => ca.IsCompleted)
            .HasDatabaseName("IX_CandidateAssessment_IsCompleted");
    }
}
