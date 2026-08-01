using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryDocumentNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoodsReceiptNumberSequences");

            migrationBuilder.DropTable(
                name: "PurchaseOrderNumberSequences");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "InventoryTransfers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "InventoryAdjustments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "CycleCounts",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_Number",
                table: "InventoryTransfers",
                column: "Number",
                unique: true,
                filter: "\"Number\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_Number",
                table: "InventoryAdjustments",
                column: "Number",
                unique: true,
                filter: "\"Number\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_CycleCounts_Number",
                table: "CycleCounts",
                column: "Number",
                unique: true,
                filter: "\"Number\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryTransfers_Number",
                table: "InventoryTransfers");

            migrationBuilder.DropIndex(
                name: "IX_InventoryAdjustments_Number",
                table: "InventoryAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_CycleCounts_Number",
                table: "CycleCounts");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "InventoryTransfers");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "InventoryAdjustments");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "CycleCounts");

            migrationBuilder.CreateTable(
                name: "GoodsReceiptNumberSequences",
                columns: table => new
                {
                    Value = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptNumberSequences", x => x.Value);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderNumberSequences",
                columns: table => new
                {
                    Value = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderNumberSequences", x => x.Value);
                });
        }
    }
}
