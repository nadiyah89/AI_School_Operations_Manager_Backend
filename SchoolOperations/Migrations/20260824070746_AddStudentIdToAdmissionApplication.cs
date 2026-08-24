using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolOperations.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentIdToAdmissionApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "AdmissionApplications",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "AdmissionApplications");
        }
    }
}
