using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SanjeshP.Payeh.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UniversityTypeId = table.Column<int>(type: "int", nullable: false),
                    Region = table.Column<int>(type: "int", nullable: false),
                    CapacityApproved = table.Column<int>(type: "int", nullable: false),
                    ParentUniversityId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Universities_Codes_UniversityTypeId",
                        column: x => x.UniversityTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Universities_Universities_ParentUniversityId",
                        column: x => x.ParentUniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExamSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    ExamModeId = table.Column<int>(type: "int", nullable: false),
                    RegistrationStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_Codes_ExamModeId",
                        column: x => x.ExamModeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exams_Codes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exams_ExamSessions_ExamSessionId",
                        column: x => x.ExamSessionId,
                        principalTable: "ExamSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamCustomFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FieldTypeId = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCustomFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamCustomFields_Codes_FieldTypeId",
                        column: x => x.FieldTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamCustomFields_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UniversityExamCapacities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocatedCapacity = table.Column<int>(type: "int", nullable: true),
                    OwnCapacity = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityExamCapacities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityExamCapacities_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UniversityExamCapacities_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UniversityExamGuestCapacities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UniversityExamCapacityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuestUniversityId = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityExamGuestCapacities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityExamGuestCapacities_Universities_GuestUniversityId",
                        column: x => x.GuestUniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UniversityExamGuestCapacities_UniversityExamCapacities_UniversityExamCapacityId",
                        column: x => x.UniversityExamCapacityId,
                        principalTable: "UniversityExamCapacities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    ExamVenueUniversityId = table.Column<int>(type: "int", nullable: false),
                    UniversityExamCapacityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UniversityExamGuestCapacityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GenderId = table.Column<int>(type: "int", nullable: true),
                    BirthCertificateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudentNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    EntryYear = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_Codes_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_Universities_ExamVenueUniversityId",
                        column: x => x.ExamVenueUniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_UniversityExamCapacities_UniversityExamCapacityId",
                        column: x => x.UniversityExamCapacityId,
                        principalTable: "UniversityExamCapacities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_UniversityExamGuestCapacities_UniversityExamGuestCapacityId",
                        column: x => x.UniversityExamGuestCapacityId,
                        principalTable: "UniversityExamGuestCapacities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamRegistrationCustomFieldValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamCustomFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRegistrationCustomFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRegistrationCustomFieldValues_ExamCustomFields_ExamCustomFieldId",
                        column: x => x.ExamCustomFieldId,
                        principalTable: "ExamCustomFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrationCustomFieldValues_ExamRegistrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamCustomFields_ExamId_DisplayOrder",
                table: "ExamCustomFields",
                columns: new[] { "ExamId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamCustomFields_ExamId_Name",
                table: "ExamCustomFields",
                columns: new[] { "ExamId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamCustomFields_FieldTypeId",
                table: "ExamCustomFields",
                column: "FieldTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrationCustomFieldValues_ExamCustomFieldId",
                table: "ExamRegistrationCustomFieldValues",
                column: "ExamCustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrationCustomFieldValues_RegistrationId_ExamCustomFieldId",
                table: "ExamRegistrationCustomFieldValues",
                columns: new[] { "RegistrationId", "ExamCustomFieldId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamId_NationalCode",
                table: "ExamRegistrations",
                columns: new[] { "ExamId", "NationalCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamId_PhoneNumber",
                table: "ExamRegistrations",
                columns: new[] { "ExamId", "PhoneNumber" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [PhoneNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamVenueUniversityId",
                table: "ExamRegistrations",
                column: "ExamVenueUniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_GenderId",
                table: "ExamRegistrations",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_UniversityExamCapacityId",
                table: "ExamRegistrations",
                column: "UniversityExamCapacityId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_UniversityExamGuestCapacityId",
                table: "ExamRegistrations",
                column: "UniversityExamGuestCapacityId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_UniversityId",
                table: "ExamRegistrations",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ExamModeId",
                table: "Exams",
                column: "ExamModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ExamSessionId",
                table: "Exams",
                column: "ExamSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ExamTypeId",
                table: "Exams",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_ExamDate",
                table: "ExamSessions",
                column: "ExamDate");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_ExamDate_StartTime_EndTime",
                table: "ExamSessions",
                columns: new[] { "ExamDate", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Universities_ParentUniversityId",
                table: "Universities",
                column: "ParentUniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_UniversityTypeId",
                table: "Universities",
                column: "UniversityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityExamCapacities_ExamId",
                table: "UniversityExamCapacities",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityExamCapacities_UniversityId_ExamId",
                table: "UniversityExamCapacities",
                columns: new[] { "UniversityId", "ExamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniversityExamGuestCapacities_GuestUniversityId",
                table: "UniversityExamGuestCapacities",
                column: "GuestUniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityExamGuestCapacities_UniversityExamCapacityId_GuestUniversityId",
                table: "UniversityExamGuestCapacities",
                columns: new[] { "UniversityExamCapacityId", "GuestUniversityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamRegistrationCustomFieldValues");

            migrationBuilder.DropTable(
                name: "ExamCustomFields");

            migrationBuilder.DropTable(
                name: "ExamRegistrations");

            migrationBuilder.DropTable(
                name: "UniversityExamGuestCapacities");

            migrationBuilder.DropTable(
                name: "UniversityExamCapacities");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Universities");

            migrationBuilder.DropTable(
                name: "ExamSessions");
        }
    }
}
