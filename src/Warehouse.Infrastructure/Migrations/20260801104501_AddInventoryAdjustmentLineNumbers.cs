using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryAdjustmentLineNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "InventoryMovements",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_InventoryAdjustmentId_LineNumber",
                table: "InventoryMovements",
                columns: new[] { "InventoryAdjustmentId", "LineNumber" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryMovements_LineNumber_Positive",
                table: "InventoryMovements",
                sql: "\"LineNumber\" IS NULL OR \"LineNumber\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_InventoryAdjustmentId_LineNumber",
                table: "InventoryMovements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryMovements_LineNumber_Positive",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "InventoryMovements");
        }
    }
}
