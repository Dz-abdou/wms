using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderLineSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrderId",
                table: "PurchaseOrderLines");

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionFactorToBaseUnit",
                table: "PurchaseOrderLines",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineAmount",
                table: "PurchaseOrderLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "PurchaseOrderLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityInBaseUnit",
                table: "PurchaseOrderLines",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrderId_LineNumber",
                table: "PurchaseOrderLines",
                columns: new[] { "PurchaseOrderId", "LineNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrderId_LineNumber",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "ConversionFactorToBaseUnit",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "LineAmount",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "QuantityInBaseUnit",
                table: "PurchaseOrderLines");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrderId",
                table: "PurchaseOrderLines",
                column: "PurchaseOrderId");
        }
    }
}
