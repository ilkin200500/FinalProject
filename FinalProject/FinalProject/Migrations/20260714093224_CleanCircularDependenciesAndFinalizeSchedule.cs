using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class CleanCircularDependenciesAndFinalizeSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courses_teachers_TeacherId",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_TeacherId",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "courses");

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "schedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_schedules_TeacherId",
                table: "schedules",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_teachers_TeacherId",
                table: "schedules",
                column: "TeacherId",
                principalTable: "teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_schedules_teachers_TeacherId",
                table: "schedules");

            migrationBuilder.DropIndex(
                name: "IX_schedules_TeacherId",
                table: "schedules");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "schedules");

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_courses_TeacherId",
                table: "courses",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_courses_teachers_TeacherId",
                table: "courses",
                column: "TeacherId",
                principalTable: "teachers",
                principalColumn: "Id");
        }
    }
}
