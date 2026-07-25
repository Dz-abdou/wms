using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterDataConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DecimalPlaces = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                    table.UniqueConstraint("AK_Currencies_Code", x => x.Code);
                    table.CheckConstraint("CK_Currencies_Code_NotBlank", "btrim(\"Code\") <> ''");
                    table.CheckConstraint("CK_Currencies_Code_Uppercase", "\"Code\" = upper(\"Code\")");
                    table.CheckConstraint("CK_Currencies_DecimalPlaces_Valid", "\"DecimalPlaces\" BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_Currencies_Default_Active", "NOT \"IsDefault\" OR \"IsActive\"");
                    table.CheckConstraint("CK_Currencies_Name_NotBlank", "btrim(\"Name\") <> ''");
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "CreatedByUserId", "DecimalPlaces", "IsActive", "IsDefault", "Name", "Symbol", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[] { new Guid("49f755f8-c6cd-4b22-8615-083b0d5536f2"), "DZD", new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, true, "Algerian dinar", "DA", new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "CreatedByUserId", "DecimalPlaces", "IsActive", "Name", "Symbol", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { new Guid("6b5c1ad6-f3a3-48e5-8222-4ca8b16a44ce"), "USD", new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, "US dollar", "$", new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("d3fa10b2-a7fc-4a75-a5f6-1d2a8efc1d96"), "EUR", new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, "Euro", "€", new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProducts_CurrencyCode",
                table: "SupplierProducts",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "UX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Currencies_OneDefault",
                table: "Currencies",
                column: "IsDefault",
                unique: true,
                filter: "\"IsDefault\" = true");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierProducts_Currencies_CurrencyCode",
                table: "SupplierProducts",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierProducts_Currencies_CurrencyCode",
                table: "SupplierProducts");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_SupplierProducts_CurrencyCode",
                table: "SupplierProducts");
        }
    }
}
