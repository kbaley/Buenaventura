using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buenaventura.Migrations
{
    /// <inheritdoc />
    public partial class AddRestrictedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "restricted",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "restricted",
                table: "users");
        }
    }
}
