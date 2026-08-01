using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentNumberingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentNumberDefinitions",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Prefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DigitCount = table.Column<int>(type: "integer", nullable: false),
                    ResetPeriod = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsManualEntry = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNumberDefinitions", x => x.Code);
                    table.CheckConstraint("CK_DocumentNumberDefinitions_Code_NotBlank", "btrim(\"Code\") <> ''");
                    table.CheckConstraint("CK_DocumentNumberDefinitions_DigitCount_Valid", "\"DigitCount\" BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_DocumentNumberDefinitions_Prefix_NotBlank", "btrim(\"Prefix\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "DocumentNumberSeries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    NextValue = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNumberSeries", x => x.Id);
                    table.CheckConstraint("CK_DocumentNumberSeries_NextValue_Positive", "\"NextValue\" > 0");
                    table.CheckConstraint("CK_DocumentNumberSeries_Year_Valid", "\"Year\" BETWEEN 2000 AND 9999");
                    table.ForeignKey(
                        name: "FK_DocumentNumberSeries_DocumentNumberDefinitions_DefinitionCo~",
                        column: x => x.DefinitionCode,
                        principalTable: "DocumentNumberDefinitions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DocumentNumberDefinitions",
                columns: new[] { "Code", "AllowsManualEntry", "Description", "DigitCount", "IsActive", "Prefix", "ResetPeriod" },
                values: new object[,]
                {
                    { "CC", false, "Cycle count", 6, true, "CC", 1 },
                    { "GR", false, "Goods receipt", 6, true, "GR", 1 },
                    { "IA", false, "Inventory adjustment", 6, true, "IA", 1 },
                    { "PO", false, "Purchase order", 6, true, "PO", 1 },
                    { "TR", false, "Inventory transfer", 6, true, "TR", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNumberSeries_DefinitionCode_Year",
                table: "DocumentNumberSeries",
                columns: new[] { "DefinitionCode", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentNumberSeries");

            migrationBuilder.DropTable(
                name: "DocumentNumberDefinitions");
        }
    }
}
