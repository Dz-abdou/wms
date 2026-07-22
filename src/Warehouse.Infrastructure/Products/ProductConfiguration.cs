using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Products;

namespace Warehouse.Infrastructure.Products;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private const string BaseUnitOfMeasureConstraint = "CK_Products_BaseUnitOfMeasure_Valid";
    private const string ConversionQuantityConstraint = "CK_ProductUnitConversions_QuantityInBaseUnit_Positive";
    private const string ConversionUnitNotBlankConstraint = "CK_ProductUnitConversions_UnitOfMeasure_NotBlank";
    private const string SkuNotBlankConstraint = "CK_Products_Sku_NotBlank";
    private const string NameNotBlankConstraint = "CK_Products_Name_NotBlank";
    private const string SkuUppercaseConstraint = "CK_Products_Sku_Uppercase";
    private const string SkuUniqueIndex = "UX_Products_Sku";

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(BaseUnitOfMeasureConstraint, "\"BaseUnitOfMeasure\" IN ('EA', 'KG', 'G', 'L', 'ML', 'M', 'CM', 'MM')");
            tableBuilder.HasCheckConstraint(SkuNotBlankConstraint, "btrim(\"Sku\") <> ''");
            tableBuilder.HasCheckConstraint(NameNotBlankConstraint, "btrim(\"Name\") <> ''");
            tableBuilder.HasCheckConstraint(SkuUppercaseConstraint, "\"Sku\" = upper(\"Sku\")");
        });

        builder.Property(product => product.BaseUnitOfMeasure).HasMaxLength(ProductUnitOfMeasure.MaxCodeLength).IsRequired();
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Sku).HasMaxLength(ProductRules.MaxSkuLength).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(ProductRules.MaxNameLength).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(ProductRules.MaxDescriptionLength);
        builder.Property(product => product.IsActive).HasDefaultValue(true).IsRequired();
        builder.OwnsMany(product => product.UnitConversions, conversion =>
        {
            conversion.ToTable("ProductUnitConversions", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(ConversionQuantityConstraint, "\"QuantityInBaseUnit\" > 0");
                tableBuilder.HasCheckConstraint(ConversionUnitNotBlankConstraint, "btrim(\"UnitOfMeasure\") <> ''");
            });
            conversion.WithOwner().HasForeignKey("ProductId");
            conversion.HasKey("ProductId", nameof(ProductUnitConversion.UnitOfMeasure));
            conversion.Property(unit => unit.UnitOfMeasure).HasMaxLength(ProductUnitOfMeasure.MaxCodeLength).IsRequired();
            conversion.Property(unit => unit.QuantityInBaseUnit).HasPrecision(18, 6).IsRequired();
        });

        builder.OwnsOne(product => product.Measurements, measurement =>
        {
        builder.Navigation(product => product.UnitConversions).HasField("unitConversions").UsePropertyAccessMode(PropertyAccessMode.Field);
            measurement.Property(details => details.NetWeight).HasPrecision(18, 3);
            measurement.Property(details => details.GrossWeight).HasPrecision(18, 3);
            measurement.Property(details => details.WeightUnitOfMeasure).HasMaxLength(ProductUnitOfMeasure.MaxCodeLength);
            measurement.Property(details => details.Length).HasPrecision(18, 3);
            measurement.Property(details => details.Width).HasPrecision(18, 3);
            measurement.Property(details => details.Height).HasPrecision(18, 3);
            measurement.Property(details => details.DimensionUnitOfMeasure).HasMaxLength(ProductUnitOfMeasure.MaxCodeLength);
            measurement.Ignore(details => details.VolumeCubicMetres);
        });

        builder.Property(product => product.CreatedByUserId).HasColumnType("uuid");
        builder.Property(product => product.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(product => product.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(product => product.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique().HasDatabaseName(SkuUniqueIndex);
        builder.HasIndex(product => product.Name).HasDatabaseName("IX_Products_Name");
    }
}