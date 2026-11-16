
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.Exam;

public class UniversityExamCapacityConfiguration
    : IEntityTypeConfiguration<UniversityExamCapacity>
{
    public void Configure(EntityTypeBuilder<UniversityExamCapacity> builder)
    {
        builder.ToTable("UniversityExamCapacities");

        builder.HasOne(x => x.University)
            .WithMany()
            .HasForeignKey(x => x.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Exam)
            .WithMany(x => x.UniversityExamCapacities)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UniversityId, x.ExamId })
            .IsUnique();

        builder.HasIndex(x => x.ExamId);
    }
}
