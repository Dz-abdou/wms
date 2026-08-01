using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryTransferId",
                table: "InventoryMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TransferredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransfers", x => x.Id);
                    table.CheckConstraint("CK_InventoryTransfers_DifferentWarehouses", "\"SourceWarehouseId\" <> \"DestinationWarehouseId\"");
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransferLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    QuantityInUnit = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    QuantityInBaseUnit = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    TransferOutMovementId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferInMovementId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransferLines", x => x.Id);
                    table.CheckConstraint("CK_InventoryTransferLines_LineNumber_Positive", "\"LineNumber\" > 0");
                    table.CheckConstraint("CK_InventoryTransferLines_QuantityInBaseUnit_Positive", "\"QuantityInBaseUnit\" > 0");
                    table.CheckConstraint("CK_InventoryTransferLines_QuantityInUnit_Positive", "\"QuantityInUnit\" > 0");
                    table.ForeignKey(
                        name: "FK_InventoryTransferLines_InventoryMovements_TransferInMovemen~",
                        column: x => x.TransferInMovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransferLines_InventoryMovements_TransferOutMoveme~",
                        column: x => x.TransferOutMovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransferLines_InventoryTransfers_InventoryTransfer~",
                        column: x => x.InventoryTransferId,
                        principalTable: "InventoryTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransferLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_InventoryTransferId",
                table: "InventoryMovements",
                column: "InventoryTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransferLines_InventoryTransferId_LineNumber",
                table: "InventoryTransferLines",
                columns: new[] { "InventoryTransferId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransferLines_InventoryTransferId_ProductId",
                table: "InventoryTransferLines",
                columns: new[] { "InventoryTransferId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransferLines_ProductId",
                table: "InventoryTransferLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransferLines_TransferInMovementId",
                table: "InventoryTransferLines",
                column: "TransferInMovementId",
                unique: true,
                filter: "\"TransferInMovementId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransferLines_TransferOutMovementId",
                table: "InventoryTransferLines",
                column: "TransferOutMovementId",
                unique: true,
                filter: "\"TransferOutMovementId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_DestinationWarehouseId_TransferredAtUtc",
                table: "InventoryTransfers",
                columns: new[] { "DestinationWarehouseId", "TransferredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_SourceWarehouseId_TransferredAtUtc",
                table: "InventoryTransfers",
                columns: new[] { "SourceWarehouseId", "TransferredAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_InventoryTransfers_InventoryTransferId",
                table: "InventoryMovements",
                column: "InventoryTransferId",
                principalTable: "InventoryTransfers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_InventoryTransfers_InventoryTransferId",
                table: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "InventoryTransferLines");

            migrationBuilder.DropTable(
                name: "InventoryTransfers");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_InventoryTransferId",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "InventoryTransferId",
                table: "InventoryMovements");
        }
    }
}
