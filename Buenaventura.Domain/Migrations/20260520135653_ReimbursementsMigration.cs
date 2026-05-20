using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buenaventura.Migrations
{
    /// <inheritdoc />
    public partial class ReimbursementsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "reimbursement_settlement_id",
                table: "transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reimbursement_settlements",
                columns: table => new
                {
                    reimbursement_settlement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    closed_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reimbursement_settlements", x => x.reimbursement_settlement_id);
                });

            migrationBuilder.CreateTable(
                name: "reimbursement_matches",
                columns: table => new
                {
                    reimbursement_match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reimbursement_settlement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    accepted_difference_reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reimbursement_matches", x => x.reimbursement_match_id);
                    table.ForeignKey(
                        name: "fk_reimbursement_matches_reimbursement_settlements_reimburseme",
                        column: x => x.reimbursement_settlement_id,
                        principalTable: "reimbursement_settlements",
                        principalColumn: "reimbursement_settlement_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reimbursement_match_transactions",
                columns: table => new
                {
                    reimbursement_match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reimbursement_match_transactions", x => new { x.reimbursement_match_id, x.transaction_id });
                    table.ForeignKey(
                        name: "fk_reimbursement_match_transactions_reimbursement_matches_reim",
                        column: x => x.reimbursement_match_id,
                        principalTable: "reimbursement_matches",
                        principalColumn: "reimbursement_match_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reimbursement_match_transactions_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "transaction_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_reimbursement_settlement_id",
                table: "transactions",
                column: "reimbursement_settlement_id");

            migrationBuilder.CreateIndex(
                name: "ix_reimbursement_match_transactions_transaction_id",
                table: "reimbursement_match_transactions",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_reimbursement_matches_reimbursement_settlement_id",
                table: "reimbursement_matches",
                column: "reimbursement_settlement_id");

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_reimbursement_settlements_reimbursement_settle",
                table: "transactions",
                column: "reimbursement_settlement_id",
                principalTable: "reimbursement_settlements",
                principalColumn: "reimbursement_settlement_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_transactions_reimbursement_settlements_reimbursement_settle",
                table: "transactions");

            migrationBuilder.DropTable(
                name: "reimbursement_match_transactions");

            migrationBuilder.DropTable(
                name: "reimbursement_matches");

            migrationBuilder.DropTable(
                name: "reimbursement_settlements");

            migrationBuilder.DropIndex(
                name: "ix_transactions_reimbursement_settlement_id",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "reimbursement_settlement_id",
                table: "transactions");
        }
    }
}
