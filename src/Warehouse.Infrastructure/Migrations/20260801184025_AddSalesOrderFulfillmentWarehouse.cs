using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderFulfillmentWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FulfillmentWarehouseCode",
                table: "SalesOrders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "FulfillmentWarehouseId",
                table: "SalesOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentWarehouseName",
                table: "SalesOrders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_FulfillmentWarehouseId_Status",
                table: "SalesOrders",
                columns: new[] { "FulfillmentWarehouseId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Warehouses_FulfillmentWarehouseId",
                table: "SalesOrders",
                column: "FulfillmentWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Warehouses_FulfillmentWarehouseId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_FulfillmentWarehouseId_Status",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "FulfillmentWarehouseCode",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "FulfillmentWarehouseId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "FulfillmentWarehouseName",
                table: "SalesOrders");
        }
    }
}
