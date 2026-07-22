using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogueEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseUnitOfMeasure",
                table: "Products",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Measurements_DimensionUnitOfMeasure",
                table: "Products",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Measurements_GrossWeight",
                table: "Products",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Measurements_Height",
                table: "Products",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Measurements_Length",
                table: "Products",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Measurements_NetWeight",
                table: "Products",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Measurements_WeightUnitOfMeasure",
                table: "Products",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Measurements_Width",
                table: "Products",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.CheckConstraint("CK_ProductCategories_Code_NotBlank", "btrim(\"Code\") <> ''");
                    table.CheckConstraint("CK_ProductCategories_Code_Uppercase", "\"Code\" = upper(\"Code\")");
                    table.CheckConstraint("CK_ProductCategories_Name_NotBlank", "btrim(\"Name\") <> ''");
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductUnitConversions",
                columns: table => new
                {
                    UnitOfMeasure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityInBaseUnit = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnitConversions", x => new { x.ProductId, x.UnitOfMeasure });
                    table.CheckConstraint("CK_ProductUnitConversions_QuantityInBaseUnit_Positive", "\"QuantityInBaseUnit\" > 0");
                    table.CheckConstraint("CK_ProductUnitConversions_UnitOfMeasure_NotBlank", "btrim(\"UnitOfMeasure\") <> ''");
                    table.ForeignKey(
                        name: "FK_ProductUnitConversions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_BaseUnitOfMeasure_Valid",
                table: "Products",
                sql: "\"BaseUnitOfMeasure\" IN ('EA', 'KG', 'G', 'L', 'ML', 'M', 'CM', 'MM')");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentCategoryId",
                table: "ProductCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "UX_ProductCategories_Code",
                table: "ProductCategories",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_CategoryId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "ProductUnitConversions");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_BaseUnitOfMeasure_Valid",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BaseUnitOfMeasure",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Measurements_DimensionUnitOfMeasure",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Measurements_GrossWeight",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Measurements_Height",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Measurements_Length",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Measurements_NetWeight",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Measurements_WeightUnitOfMeasure",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Measurements_Width",
                table: "Products");
        }
    }
}
