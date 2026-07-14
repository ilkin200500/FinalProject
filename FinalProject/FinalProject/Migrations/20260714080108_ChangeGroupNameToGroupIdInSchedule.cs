using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGroupNameToGroupIdInSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "schedules");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "schedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_schedules_GroupId",
                table: "schedules",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_Groups_GroupId",
                table: "schedules",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_schedules_Groups_GroupId",
                table: "schedules");

            migrationBuilder.DropIndex(
                name: "IX_schedules_GroupId",
                table: "schedules");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "schedules");

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "schedules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
