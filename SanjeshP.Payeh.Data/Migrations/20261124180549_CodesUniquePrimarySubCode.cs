using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SanjeshP.Payeh.Data.Migrations
{
    /// <inheritdoc />
    public partial class CodesUniquePrimarySubCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ردیف‌های تکراری (PrimaryCode, SubCode): کوچک‌ترین Id می‌ماند، مراجع به آن منتقل و بقیه حذف نرم می‌شوند.
            migrationBuilder.Sql(@"
SELECT c.Id AS DupId, k.KeepId INTO #CodeDup
FROM Codes c
JOIN (SELECT PrimaryCode, SubCode, MIN(Id) AS KeepId FROM Codes
      WHERE IsDeleted = 0 AND PrimaryCode IS NOT NULL AND SubCode IS NOT NULL
      GROUP BY PrimaryCode, SubCode HAVING COUNT(*) > 1) k
  ON k.PrimaryCode = c.PrimaryCode AND k.SubCode = c.SubCode
WHERE c.IsDeleted = 0 AND c.Id <> k.KeepId;

IF OBJECT_ID(N'Exams') IS NOT NULL
BEGIN
    UPDATE e SET ExamTypeId = d.KeepId FROM Exams e JOIN #CodeDup d ON d.DupId = e.ExamTypeId;
    UPDATE e SET ExamModeId = d.KeepId FROM Exams e JOIN #CodeDup d ON d.DupId = e.ExamModeId;
END
IF OBJECT_ID(N'Universities') IS NOT NULL
    UPDATE u SET UniversityTypeId = d.KeepId FROM Universities u JOIN #CodeDup d ON d.DupId = u.UniversityTypeId;
IF OBJECT_ID(N'ExamRegistrations') IS NOT NULL
    UPDATE r SET GenderId = d.KeepId FROM ExamRegistrations r JOIN #CodeDup d ON d.DupId = r.GenderId;
IF OBJECT_ID(N'ExamCustomFields') IS NOT NULL
    UPDATE f SET FieldTypeId = d.KeepId FROM ExamCustomFields f JOIN #CodeDup d ON d.DupId = f.FieldTypeId;
UPDATE c SET ParentId = d.KeepId FROM Codes c JOIN #CodeDup d ON d.DupId = c.ParentId;

UPDATE c SET IsDeleted = 1, IsActive = 0, UpdatedDate = GETUTCDATE() FROM Codes c JOIN #CodeDup d ON d.DupId = c.Id;
DROP TABLE #CodeDup;");

            migrationBuilder.CreateIndex(
                name: "UX_Codes_PrimaryCode_SubCode",
                table: "Codes",
                columns: new[] { "PrimaryCode", "SubCode" },
                unique: true,
                filter: "[PrimaryCode] IS NOT NULL AND [SubCode] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Codes_PrimaryCode_SubCode",
                table: "Codes");
        }
    }
}
