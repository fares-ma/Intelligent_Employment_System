using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class ProcessedIdempotencyKeyConfig : IEntityTypeConfiguration<ProcessedIdempotencyKey>
{
    public void Configure(EntityTypeBuilder<ProcessedIdempotencyKey> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique();

        builder.Property(x => x.Source)
            .HasMaxLength(64)
            .HasDefaultValue("TalentXWebhook");

        builder.Property(x => x.ProcessedAtUtc)
            .HasDefaultValueSql("getutcdate()");
    }
}
