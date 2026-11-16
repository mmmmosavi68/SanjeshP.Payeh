
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.Exam;

public class ExamRegistrationCustomFieldValueConfiguration
    : IEntityTypeConfiguration<ExamRegistrationCustomFieldValue>
{
    public void Configure(
        EntityTypeBuilder<ExamRegistrationCustomFieldValue> builder)
    {
        builder.ToTable("ExamRegistrationCustomFieldValues");

        builder.HasOne(x => x.Registration)
            .WithMany(x => x.CustomFieldValues)
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ExamCustomField)
            .WithMany(x => x.RegistrationValues)
            .HasForeignKey(x => x.ExamCustomFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        // هر فیلد اختصاصی برای هر ثبت‌نام فقط یک مقدار داشته باشد.
        builder.HasIndex(x => new
        {
            x.RegistrationId,
            x.ExamCustomFieldId
        }).IsUnique();

        builder.HasIndex(x => x.ExamCustomFieldId);
    }
}
