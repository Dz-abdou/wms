using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CycleCountId",
                table: "InventoryMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CycleCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CountedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CycleCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CycleCounts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CycleCountLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleCountId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemQuantityInBase = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    SystemBalanceVersion = table.Column<int>(type: "integer", nullable: false),
                    CountedUnitOfMeasure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CountedQuantityInUnit = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    CountedQuantityInBase = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    VarianceQuantityInBase = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    InventoryMovementId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CycleCountLines", x => x.Id);
                    table.CheckConstraint("CK_CycleCountLines_CountedQuantityInBase_NonNegative", "\"CountedQuantityInBase\" >= 0");
                    table.CheckConstraint("CK_CycleCountLines_CountedQuantityInUnit_NonNegative", "\"CountedQuantityInUnit\" >= 0");
                    table.CheckConstraint("CK_CycleCountLines_LineNumber_Positive", "\"LineNumber\" > 0");
                    table.CheckConstraint("CK_CycleCountLines_SystemBalanceVersion_NonNegative", "\"SystemBalanceVersion\" >= 0");
                    table.CheckConstraint("CK_CycleCountLines_SystemQuantity_NonNegative", "\"SystemQuantityInBase\" >= 0");
                    table.ForeignKey(
                        name: "FK_CycleCountLines_CycleCounts_CycleCountId",
                        column: x => x.CycleCountId,
                        principalTable: "CycleCounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CycleCountLines_InventoryMovements_InventoryMovementId",
                        column: x => x.InventoryMovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CycleCountLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CycleCountId",
                table: "InventoryMovements",
                column: "CycleCountId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleCountLines_CycleCountId_LineNumber",
                table: "CycleCountLines",
                columns: new[] { "CycleCountId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CycleCountLines_CycleCountId_ProductId",
                table: "CycleCountLines",
                columns: new[] { "CycleCountId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CycleCountLines_InventoryMovementId",
                table: "CycleCountLines",
                column: "InventoryMovementId",
                unique: true,
                filter: "\"InventoryMovementId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CycleCountLines_ProductId",
                table: "CycleCountLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleCounts_WarehouseId_CountedAtUtc",
                table: "CycleCounts",
                columns: new[] { "WarehouseId", "CountedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_CycleCounts_CycleCountId",
                table: "InventoryMovements",
                column: "CycleCountId",
                principalTable: "CycleCounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_CycleCounts_CycleCountId",
                table: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "CycleCountLines");

            migrationBuilder.DropTable(
                name: "CycleCounts");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_CycleCountId",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "CycleCountId",
                table: "InventoryMovements");
        }
    }
}
