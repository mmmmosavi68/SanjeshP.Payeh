
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SanjeshP.Payeh.Entities.University;

namespace SanjeshP.Payeh.Entities.User;

public class UserSessionConfiguration
    : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasOne(x => x.ActiveUniversity)
            .WithMany()
            .HasForeignKey(x => x.ActiveUniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ActiveUniversityId);
    }
}
