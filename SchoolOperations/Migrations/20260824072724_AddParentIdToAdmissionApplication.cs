using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolOperations.Migrations
{
    /// <inheritdoc />
    public partial class AddParentIdToAdmissionApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "AdmissionApplications",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "AdmissionApplications");
        }
    }
}
