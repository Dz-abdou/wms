using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Products;

namespace Warehouse.Infrastructure.Products;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private const string SkuNotBlankConstraint = "CK_Products_Sku_NotBlank";
    private const string NameNotBlankConstraint = "CK_Products_Name_NotBlank";
    private const string SkuUppercaseConstraint = "CK_Products_Sku_Uppercase";
    private const string SkuUniqueIndex = "UX_Products_Sku";

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(SkuNotBlankConstraint, "btrim(\"Sku\") <> ''");
            tableBuilder.HasCheckConstraint(NameNotBlankConstraint, "btrim(\"Name\") <> ''");
            tableBuilder.HasCheckConstraint(SkuUppercaseConstraint, "\"Sku\" = upper(\"Sku\")");
        });

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Sku).HasMaxLength(ProductRules.MaxSkuLength).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(ProductRules.MaxNameLength).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(ProductRules.MaxDescriptionLength);
        builder.Property(product => product.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(product => product.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(product => product.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique().HasDatabaseName(SkuUniqueIndex);
        builder.HasIndex(product => product.Name).HasDatabaseName("IX_Products_Name");
    }
}