using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourseAndGradeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LetterGrade",
                table: "grades");

            migrationBuilder.RenameColumn(
                name: "Total",
                table: "grades",
                newName: "TeacherId");

            migrationBuilder.AlterColumn<int>(
                name: "Final",
                table: "grades",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "grades",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "CourseCode",
                table: "courses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_grades_TeacherId",
                table: "grades",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_grades_teachers_TeacherId",
                table: "grades",
                column: "TeacherId",
                principalTable: "teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_grades_teachers_TeacherId",
                table: "grades");

            migrationBuilder.DropIndex(
                name: "IX_grades_TeacherId",
                table: "grades");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "grades");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "grades",
                newName: "Total");

            migrationBuilder.AlterColumn<int>(
                name: "Final",
                table: "grades",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LetterGrade",
                table: "grades",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CourseCode",
                table: "courses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
