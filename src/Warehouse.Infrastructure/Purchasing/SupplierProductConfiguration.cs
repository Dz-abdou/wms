using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Products;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Suppliers;
using Warehouse.Domain.Currencies;

namespace Warehouse.Infrastructure.Purchasing;

public sealed class SupplierProductConfiguration : IEntityTypeConfiguration<SupplierProduct>
{
    public void Configure(EntityTypeBuilder<SupplierProduct> builder)
    {
        builder.ToTable("SupplierProducts", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_SupplierProducts_MinimumOrderQuantity_Positive", "\"MinimumOrderQuantity\" > 0");
            tableBuilder.HasCheckConstraint("CK_SupplierProducts_UnitPrice_NonNegative", "\"UnitPrice\" >= 0");
            tableBuilder.HasCheckConstraint("CK_SupplierProducts_CurrencyCode_Uppercase", "\"CurrencyCode\" = upper(\"CurrencyCode\")");
        });

        builder.HasKey(supplierProduct => supplierProduct.Id);
        builder.Property(supplierProduct => supplierProduct.SupplierId).HasColumnType("uuid").IsRequired();
        builder.Property(supplierProduct => supplierProduct.ProductId).HasColumnType("uuid").IsRequired();
        builder.Property(supplierProduct => supplierProduct.SupplierSku).HasMaxLength(SupplierProductRules.MaxSupplierSkuLength);
        builder.Property(supplierProduct => supplierProduct.PurchaseUnitOfMeasure).HasMaxLength(SupplierProductRules.UnitOfMeasureLength).IsRequired();
        builder.Property(supplierProduct => supplierProduct.MinimumOrderQuantity).HasPrecision(18, 6).IsRequired();
        builder.Property(supplierProduct => supplierProduct.UnitPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(supplierProduct => supplierProduct.CurrencyCode).HasMaxLength(SupplierProductRules.CurrencyCodeLength).IsRequired();
        builder.Property(supplierProduct => supplierProduct.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(supplierProduct => supplierProduct.CreatedByUserId).HasColumnType("uuid");
        builder.Property(supplierProduct => supplierProduct.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(supplierProduct => supplierProduct.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(supplierProduct => supplierProduct.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();

        builder.HasIndex(supplierProduct => new { supplierProduct.SupplierId, supplierProduct.ProductId, supplierProduct.PurchaseUnitOfMeasure })
            .IsUnique()
            .HasDatabaseName("UX_SupplierProducts_Supplier_Product_Unit");
        builder.HasIndex(supplierProduct => supplierProduct.ProductId);
        builder.HasOne<Supplier>().WithMany().HasForeignKey(supplierProduct => supplierProduct.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(supplierProduct => supplierProduct.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Currency>().WithMany().HasPrincipalKey(currency => currency.Code).HasForeignKey(supplierProduct => supplierProduct.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
    }
}
