using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buenaventura.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tags",
                table: "transactions",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tags",
                table: "transactions");
        }
    }
}
