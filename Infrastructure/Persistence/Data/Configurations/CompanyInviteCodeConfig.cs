using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class CompanyInviteCodeConfig : IEntityTypeConfiguration<CompanyInviteCode>
{
    public void Configure(EntityTypeBuilder<CompanyInviteCode> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.Company)
            .WithMany(co => co.CompanyInviteCodes)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.CreatedByRecruiter)
            .WithMany()
            .HasForeignKey(c => c.CreatedByRecruiterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(6);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("getutcdate()");

        builder.HasIndex(c => new { c.CompanyId, c.IsActive });
    }
}
