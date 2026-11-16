
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.Exam;

public class UniversityExamGuestCapacityConfiguration
    : IEntityTypeConfiguration<UniversityExamGuestCapacity>
{
    public void Configure(EntityTypeBuilder<UniversityExamGuestCapacity> builder)
    {
        builder.ToTable("UniversityExamGuestCapacities");

        builder.HasOne(x => x.UniversityExamCapacity)
            .WithMany(x => x.GuestCapacities)
            .HasForeignKey(x => x.UniversityExamCapacityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GuestUniversity)
            .WithMany()
            .HasForeignKey(x => x.GuestUniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        // هر دانشگاه مهمان برای هر تخصیص ظرفیت فقط یک بار ثبت شود.
        builder.HasIndex(x => new
        {
            x.UniversityExamCapacityId,
            x.GuestUniversityId
        }).IsUnique();

        builder.HasIndex(x => x.GuestUniversityId);
    }
}
