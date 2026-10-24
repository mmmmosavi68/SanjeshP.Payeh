using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SanjeshP.Payeh.Data.Migrations
{
    /// <inheritdoc />
    public partial class addversioninMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "version",
                table: "Menus",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version",
                table: "Menus");
        }
    }
}
