using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolOperations.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueApplicationUserLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Attendances");
        }
    }
}
