using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // TPH discriminator
        builder.HasDiscriminator<string>("UserType")
            .HasValue<CandidateUser>("Candidate")
            .HasValue<Recruiter>("Recruiter");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.ProfilePicturePath)
            .HasMaxLength(500);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("getutcdate()");
    }
}
