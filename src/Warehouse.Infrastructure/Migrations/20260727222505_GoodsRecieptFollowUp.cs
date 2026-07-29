using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GoodsRecieptFollowUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_WarehouseId",
                table: "GoodsReceipts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLines_InventoryMovementId",
                table: "GoodsReceiptLines",
                column: "InventoryMovementId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_GoodsReceiptLines_AcceptedQuantity_Positive",
                table: "GoodsReceiptLines",
                sql: "\"AcceptedQuantity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GoodsReceiptLines_AcceptedQuantityInBaseUnit_Positive",
                table: "GoodsReceiptLines",
                sql: "\"AcceptedQuantityInBaseUnit\" > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceiptLines_InventoryMovements_InventoryMovementId",
                table: "GoodsReceiptLines",
                column: "InventoryMovementId",
                principalTable: "InventoryMovements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_PurchaseOrders_PurchaseOrderId",
                table: "GoodsReceipts",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Warehouses_WarehouseId",
                table: "GoodsReceipts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceiptLines_InventoryMovements_InventoryMovementId",
                table: "GoodsReceiptLines");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_PurchaseOrders_PurchaseOrderId",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Warehouses_WarehouseId",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_WarehouseId",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceiptLines_InventoryMovementId",
                table: "GoodsReceiptLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GoodsReceiptLines_AcceptedQuantity_Positive",
                table: "GoodsReceiptLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GoodsReceiptLines_AcceptedQuantityInBaseUnit_Positive",
                table: "GoodsReceiptLines");
        }
    }
}
