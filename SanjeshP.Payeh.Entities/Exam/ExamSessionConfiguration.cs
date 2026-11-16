
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.Exam;

public class ExamSessionConfiguration : IEntityTypeConfiguration<ExamSession>
{
    public void Configure(EntityTypeBuilder<ExamSession> builder)
    {
        builder.ToTable("ExamSessions");

        // بهینه‌سازی جست‌وجوی نوبت‌ها بر اساس تاریخ برگزاری
        builder.HasIndex(x => x.ExamDate);

        // بهینه‌سازی جست‌وجو بر اساس تاریخ و بازه زمانی
        builder.HasIndex(x => new
        {
            x.ExamDate,
            x.StartTime,
            x.EndTime
        });
    }
}
