using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SanjeshP.Payeh.Data.Migrations
{
    /// <inheritdoc />
    public partial class GroupSdite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_CreatorId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CreatorId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Groups");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Groups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_UserId",
                table: "Groups",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_UserId",
                table: "Groups",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_UserId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_UserId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Groups");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Groups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatorId",
                table: "Groups",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_CreatorId",
                table: "Groups",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
