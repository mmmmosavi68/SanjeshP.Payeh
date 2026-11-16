
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.University;

public class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> builder)
    {
        builder.ToTable("Universities");

        // رابطه با نوع دانشگاه از جدول Codes
        builder.HasOne(x => x.UniversityType)
            .WithMany()
            .HasForeignKey(x => x.UniversityTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        // رابطه خودارجاعی دانشگاه و پردیس‌های زیرمجموعه
        builder.HasOne(x => x.ParentUniversity)
            .WithMany(x => x.ChildUniversities)
            .HasForeignKey(x => x.ParentUniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ایندکس برای جستجوی دانشگاه‌های مادر و پردیس‌ها
        builder.HasIndex(x => x.ParentUniversityId);

        // ایندکس برای جستجو بر اساس نوع دانشگاه
        builder.HasIndex(x => x.UniversityTypeId);
    }
}
