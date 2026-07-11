using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_teachers_TeacherId",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_teachers_Department_DepartmentId",
                table: "teachers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Department",
                table: "Department");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Course",
                table: "Course");

            migrationBuilder.RenameTable(
                name: "Department",
                newName: "departments");

            migrationBuilder.RenameTable(
                name: "Course",
                newName: "courses");

            migrationBuilder.RenameIndex(
                name: "IX_Course_TeacherId",
                table: "courses",
                newName: "IX_courses_TeacherId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_departments",
                table: "departments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_courses",
                table: "courses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_courses_teachers_TeacherId",
                table: "courses",
                column: "TeacherId",
                principalTable: "teachers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_teachers_departments_DepartmentId",
                table: "teachers",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courses_teachers_TeacherId",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "FK_teachers_departments_DepartmentId",
                table: "teachers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_departments",
                table: "departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_courses",
                table: "courses");

            migrationBuilder.RenameTable(
                name: "departments",
                newName: "Department");

            migrationBuilder.RenameTable(
                name: "courses",
                newName: "Course");

            migrationBuilder.RenameIndex(
                name: "IX_courses_TeacherId",
                table: "Course",
                newName: "IX_Course_TeacherId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Department",
                table: "Department",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Course",
                table: "Course",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_teachers_TeacherId",
                table: "Course",
                column: "TeacherId",
                principalTable: "teachers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_teachers_Department_DepartmentId",
                table: "teachers",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "Id");
        }
    }
}
