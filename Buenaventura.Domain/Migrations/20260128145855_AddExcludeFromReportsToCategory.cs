using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buenaventura.Migrations
{
    /// <inheritdoc />
    public partial class AddExcludeFromReportsToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "exclude_from_transaction_report",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exclude_from_transaction_report",
                table: "categories");
        }
    }
}
