using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddNullableSpecialityToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecialityId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_SpecialityId",
                table: "Groups",
                column: "SpecialityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_specialities_SpecialityId",
                table: "Groups",
                column: "SpecialityId",
                principalTable: "specialities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_specialities_SpecialityId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_SpecialityId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "SpecialityId",
                table: "Groups");
        }
    }
}
