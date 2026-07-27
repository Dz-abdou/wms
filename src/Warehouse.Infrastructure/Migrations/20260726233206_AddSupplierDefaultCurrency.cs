using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierDefaultCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultCurrencyCode",
                table: "Suppliers",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "DZD");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Suppliers_DefaultCurrencyCode_Uppercase",
                table: "Suppliers",
                sql: "\"DefaultCurrencyCode\" = upper(\"DefaultCurrencyCode\")");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Suppliers_DefaultCurrencyCode_Uppercase",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DefaultCurrencyCode",
                table: "Suppliers");
        }
    }
}
