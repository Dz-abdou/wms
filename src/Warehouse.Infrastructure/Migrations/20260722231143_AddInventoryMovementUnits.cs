using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryMovementUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowsFractionalQuantity",
                table: "ProductUnitConversions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityDeltaInUnit",
                table: "InventoryMovements",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "InventoryMovements",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryMovements_QuantityDeltaInUnit_NonZero",
                table: "InventoryMovements",
                sql: "\"QuantityDeltaInUnit\" <> 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryMovements_QuantityDeltaInUnit_NonZero",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "AllowsFractionalQuantity",
                table: "ProductUnitConversions");

            migrationBuilder.DropColumn(
                name: "QuantityDeltaInUnit",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "InventoryMovements");
        }
    }
}
