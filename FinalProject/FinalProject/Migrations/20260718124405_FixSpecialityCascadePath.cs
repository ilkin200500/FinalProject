using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class FixSpecialityCascadePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Speciality",
                table: "students");

            migrationBuilder.AddColumn<int>(
                name: "SpecialityId",
                table: "students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpecialityId1",
                table: "students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialityId",
                table: "courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpecialityId1",
                table: "courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "specialities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specialities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_students_SpecialityId",
                table: "students",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_students_SpecialityId1",
                table: "students",
                column: "SpecialityId1");

            migrationBuilder.CreateIndex(
                name: "IX_courses_SpecialityId",
                table: "courses",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_courses_SpecialityId1",
                table: "courses",
                column: "SpecialityId1");

            migrationBuilder.AddForeignKey(
                name: "FK_courses_specialities_SpecialityId",
                table: "courses",
                column: "SpecialityId",
                principalTable: "specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_courses_specialities_SpecialityId1",
                table: "courses",
                column: "SpecialityId1",
                principalTable: "specialities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_students_specialities_SpecialityId",
                table: "students",
                column: "SpecialityId",
                principalTable: "specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_students_specialities_SpecialityId1",
                table: "students",
                column: "SpecialityId1",
                principalTable: "specialities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courses_specialities_SpecialityId",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "FK_courses_specialities_SpecialityId1",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "FK_students_specialities_SpecialityId",
                table: "students");

            migrationBuilder.DropForeignKey(
                name: "FK_students_specialities_SpecialityId1",
                table: "students");

            migrationBuilder.DropTable(
                name: "specialities");

            migrationBuilder.DropIndex(
                name: "IX_students_SpecialityId",
                table: "students");

            migrationBuilder.DropIndex(
                name: "IX_students_SpecialityId1",
                table: "students");

            migrationBuilder.DropIndex(
                name: "IX_courses_SpecialityId",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_SpecialityId1",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "SpecialityId",
                table: "students");

            migrationBuilder.DropColumn(
                name: "SpecialityId1",
                table: "students");

            migrationBuilder.DropColumn(
                name: "SpecialityId",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "SpecialityId1",
                table: "courses");

            migrationBuilder.AddColumn<string>(
                name: "Speciality",
                table: "students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
