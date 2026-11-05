using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buenaventura.Migrations
{
    /// <inheritdoc />
    public partial class PreserveImportedTransactionDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_imported",
                table: "transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Older imports can be identified by their bank download reference.
            migrationBuilder.Sql("""
                UPDATE transactions
                SET is_imported = TRUE
                WHERE download_id IS NOT NULL AND download_id <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_imported",
                table: "transactions");
        }
    }
}
