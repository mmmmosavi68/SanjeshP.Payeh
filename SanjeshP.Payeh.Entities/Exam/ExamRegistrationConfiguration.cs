
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.Exam;

public class ExamRegistrationConfiguration
    : IEntityTypeConfiguration<ExamRegistration>
{
    public void Configure(EntityTypeBuilder<ExamRegistration> builder)
    {
        builder.ToTable("ExamRegistrations");

        builder.HasOne(x => x.Exam)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.University)
            .WithMany()
            .HasForeignKey(x => x.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ExamVenueUniversity)
            .WithMany()
            .HasForeignKey(x => x.ExamVenueUniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UniversityExamCapacity)
            .WithMany()
            .HasForeignKey(x => x.UniversityExamCapacityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UniversityExamGuestCapacity)
            .WithMany()
            .HasForeignKey(x => x.UniversityExamGuestCapacityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Gender)
            .WithMany()
            .HasForeignKey(x => x.GenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ExamId);
        builder.HasIndex(x => x.UniversityId);
        builder.HasIndex(x => x.ExamVenueUniversityId);
        builder.HasIndex(x => x.UniversityExamCapacityId);
        builder.HasIndex(x => x.UniversityExamGuestCapacityId);
        builder.HasIndex(x => x.NationalCode);
    }
}
