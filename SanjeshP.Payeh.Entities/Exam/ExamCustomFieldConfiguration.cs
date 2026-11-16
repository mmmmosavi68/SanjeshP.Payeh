
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.Exam;

public class ExamCustomFieldConfiguration
    : IEntityTypeConfiguration<ExamCustomField>
{
    public void Configure(EntityTypeBuilder<ExamCustomField> builder)
    {
        builder.ToTable("ExamCustomFields");

        builder.HasOne(x => x.Exam)
            .WithMany(x => x.CustomFields)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FieldType)
            .WithMany()
            .HasForeignKey(x => x.FieldTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // نام هر فیلد در یک آزمون باید یکتا باشد.
        builder.HasIndex(x => new { x.ExamId, x.Name })
            .IsUnique();

        builder.HasIndex(x => x.FieldTypeId);
        builder.HasIndex(x => new { x.ExamId, x.DisplayOrder });
    }
}
