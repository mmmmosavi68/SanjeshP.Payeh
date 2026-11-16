
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.Exam;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams");

        builder.HasOne(x => x.ExamSession)
            .WithMany(x => x.Exams)
            .HasForeignKey(x => x.ExamSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ExamType)
            .WithMany()
            .HasForeignKey(x => x.ExamTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ExamMode)
            .WithMany()
            .HasForeignKey(x => x.ExamModeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ExamSessionId);
        builder.HasIndex(x => x.ExamTypeId);
        builder.HasIndex(x => x.ExamModeId);
    }
}
