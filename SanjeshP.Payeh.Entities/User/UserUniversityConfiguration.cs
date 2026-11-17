
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.User;

public class UserUniversityConfiguration
    : IEntityTypeConfiguration<UserUniversity>
{
    public void Configure(EntityTypeBuilder<UserUniversity> builder)
    {
        builder.ToTable("UserUniversities");

        builder.HasOne(x => x.User)
            .WithMany(x => x.UniversityAccesses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.University)
            .WithMany()
            .HasForeignKey(x => x.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        // هر کاربر فقط یک ارتباط با هر دانشگاه داشته باشد.
        // برای لغو دسترسی، رکورد حذف نرم می‌شود و در صورت نیاز
        // همان رکورد مجدداً فعال خواهد شد.
        builder.HasIndex(x => new
        {
            x.UserId,
            x.UniversityId
        }).IsUnique();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsActive
        });
    }
}
