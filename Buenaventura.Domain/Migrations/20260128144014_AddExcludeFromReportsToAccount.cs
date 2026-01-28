using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buenaventura.Migrations
{
    /// <inheritdoc />
    public partial class AddExcludeFromReportsToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "exclude_from_reports",
                table: "accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exclude_from_reports",
                table: "accounts");
        }
    }
}
