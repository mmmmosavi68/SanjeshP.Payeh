using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SanjeshP.Payeh.Entities.Menu;

public class CodeConfiguration : IEntityTypeConfiguration<Code>
{
    public void Configure(EntityTypeBuilder<Code> builder)
    {
        // هر زوج (PrimaryCode, SubCode) در رکوردهای حذف‌نشده یکتاست.
        builder.HasIndex(x => new { x.PrimaryCode, x.SubCode })
            .IsUnique()
            .HasFilter("[PrimaryCode] IS NOT NULL AND [SubCode] IS NOT NULL AND [IsDeleted] = 0")
            .HasDatabaseName("UX_Codes_PrimaryCode_SubCode");
    }
}
