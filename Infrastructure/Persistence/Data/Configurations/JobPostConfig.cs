using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class JobPostConfig : IEntityTypeConfiguration<JobPost>
{
    public void Configure(EntityTypeBuilder<JobPost> builder)
    {
        // ── Primary Key ──
        builder.HasKey(j => j.Id);

        // ── Foreign Keys with cascade deletion restrictions ──
        // Company: Restrict (to prevent multiple cascade paths when Company is deleted)
        builder.HasOne(j => j.Company)
            .WithMany(c => c.JobPosts)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // CreatedByRecruiter: Cascade (orphan if recruiter deleted)
        builder.HasOne(j => j.CreatedByRecruiter)
            .WithMany(r => r.CreatedJobPosts)
            .HasForeignKey(j => j.CreatedByRecruiterId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Unique Constraints ──
        // (None defined per data-model.md)

        // ── Indexes ──
        builder.HasIndex(j => new { j.IsPublished, j.IsActive, j.ExpiryDate });
        builder.HasIndex(j => j.CompanyId);
        builder.HasIndex(j => j.CreatedByRecruiterId);

        // ── Field Constraints ──
        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(j => j.Description)
            .IsRequired();

        builder.Property(j => j.Location)
            .HasMaxLength(500);

        builder.Property(j => j.Currency)
            .HasMaxLength(10);

        builder.Property(j => j.SalaryMin)
            .HasPrecision(18, 2);

        builder.Property(j => j.SalaryMax)
            .HasPrecision(18, 2);

        builder.Property(j => j.IsPublished)
            .HasDefaultValue(false);

        builder.Property(j => j.IsActive)
            .HasDefaultValue(true);

        builder.Property(j => j.DeletedAt)
            .IsRequired(false);

        builder.Property(j => j.DeletedBy)
            .HasMaxLength(450);

        builder.Property(j => j.CreatedAt)
            .HasDefaultValueSql("getutcdate()");
    }
}
