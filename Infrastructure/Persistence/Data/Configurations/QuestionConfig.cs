using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class QuestionConfig : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.HasOne(q => q.Assessment)
            .WithMany(a => a.Questions)
            .HasForeignKey(q => q.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(q => q.Text)
            .IsRequired();

        builder.Property(q => q.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(q => q.Points)
            .HasDefaultValue(0);

        builder.HasIndex(q => q.AssessmentId);
    }
}
