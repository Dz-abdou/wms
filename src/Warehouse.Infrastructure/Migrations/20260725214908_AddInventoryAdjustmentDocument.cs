using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryAdjustmentDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryAdjustmentId",
                table: "InventoryMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_InventoryAdjustmentId",
                table: "InventoryMovements",
                column: "InventoryAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_CreatedAtUtc",
                table: "InventoryAdjustments",
                column: "CreatedAtUtc");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_InventoryAdjustments_InventoryAdjustment~",
                table: "InventoryMovements",
                column: "InventoryAdjustmentId",
                principalTable: "InventoryAdjustments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_InventoryAdjustments_InventoryAdjustment~",
                table: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "InventoryAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_InventoryAdjustmentId",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "InventoryAdjustmentId",
                table: "InventoryMovements");
        }
    }
}
