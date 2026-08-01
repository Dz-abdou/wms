using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShippingAddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShippingAddressLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingAddressLine1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShippingAddressLine2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShippingAddressCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingAddressPostalCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ShippingAddressCountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ShippingAddressInstructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestedShipDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CustomerReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeliveryInstructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.Id);
                    table.CheckConstraint("CK_SalesOrders_CurrencyCode_Uppercase", "\"CurrencyCode\" = upper(\"CurrencyCode\")");
                    table.CheckConstraint("CK_SalesOrders_Number_NotBlank", "btrim(\"Number\") <> ''");
                    table.CheckConstraint("CK_SalesOrders_Status_Valid", "\"Status\" IN (0, 1, 2)");
                    table.ForeignKey(
                        name: "FK_SalesOrders_CustomerAddresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "CustomerAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductSku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    QuantityInBaseUnit = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ConversionFactorToBaseUnit = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderLines", x => x.Id);
                    table.CheckConstraint("CK_SalesOrderLines_Quantity_Positive", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_SalesOrderLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderLines_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStatus = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderStatusHistory_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DocumentNumberDefinitions",
                columns: new[] { "Code", "AllowsManualEntry", "Description", "DigitCount", "IsActive", "Prefix", "ResetPeriod" },
                values: new object[] { "SO", false, "Sales order", 6, true, "SO", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderLines_ProductId",
                table: "SalesOrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderLines_SalesOrderId_LineNumber",
                table: "SalesOrderLines",
                columns: new[] { "SalesOrderId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerId_Status",
                table: "SalesOrders",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_Number",
                table: "SalesOrders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_ShippingAddressId",
                table: "SalesOrders",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderStatusHistory_SalesOrderId",
                table: "SalesOrderStatusHistory",
                column: "SalesOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOrderLines");

            migrationBuilder.DropTable(
                name: "SalesOrderStatusHistory");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DeleteData(
                table: "DocumentNumberDefinitions",
                keyColumn: "Code",
                keyValue: "SO");
        }
    }
}
