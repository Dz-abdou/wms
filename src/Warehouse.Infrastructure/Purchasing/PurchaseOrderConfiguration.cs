using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Products;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Suppliers;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Infrastructure.Purchasing;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_PurchaseOrders_Status_Valid", "\"Status\" IN (0, 1, 2, 3, 4)");
        });

        builder.HasKey(purchaseOrder => purchaseOrder.Id);
        builder.Property(purchaseOrder => purchaseOrder.SupplierId).HasColumnType("uuid").IsRequired();
        builder.Property(purchaseOrder => purchaseOrder.Number).HasMaxLength(PurchaseOrderRules.MaxNumberLength);
        builder.Property(purchaseOrder => purchaseOrder.DestinationWarehouseId).HasColumnType("uuid");
        builder.Property(purchaseOrder => purchaseOrder.CurrencyCode).HasMaxLength(SupplierProductRules.CurrencyCodeLength);
        builder.Property(purchaseOrder => purchaseOrder.OrderDate).HasColumnType("date");
        builder.Property(purchaseOrder => purchaseOrder.ExpectedDeliveryDate).HasColumnType("date");
        builder.Property(purchaseOrder => purchaseOrder.BuyerUserId).HasColumnType("uuid");
        builder.Property(purchaseOrder => purchaseOrder.SupplierReference).HasMaxLength(PurchaseOrderRules.MaxSupplierReferenceLength);
        builder.Property(purchaseOrder => purchaseOrder.Notes).HasMaxLength(PurchaseOrderRules.MaxNotesLength);
        builder.Property(purchaseOrder => purchaseOrder.SubmittedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(purchaseOrder => purchaseOrder.Version).IsConcurrencyToken().IsRequired();
        builder.Property(purchaseOrder => purchaseOrder.Status).HasConversion<int>().IsRequired();
        builder.Property(purchaseOrder => purchaseOrder.CreatedByUserId).HasColumnType("uuid");
        builder.Property(purchaseOrder => purchaseOrder.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(purchaseOrder => purchaseOrder.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(purchaseOrder => purchaseOrder.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(purchaseOrder => new { purchaseOrder.SupplierId, purchaseOrder.Status });
        builder.HasIndex(purchaseOrder => purchaseOrder.Number).IsUnique();
        builder.HasOne<Supplier>().WithMany().HasForeignKey(purchaseOrder => purchaseOrder.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(purchaseOrder => purchaseOrder.DestinationWarehouseId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(purchaseOrder => purchaseOrder.Lines, line =>
        {
            line.ToTable("PurchaseOrderLines", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_PurchaseOrderLines_Quantity_Positive", "\"Quantity\" > 0");
                tableBuilder.HasCheckConstraint("CK_PurchaseOrderLines_UnitPrice_NonNegative", "\"UnitPrice\" >= 0");
                tableBuilder.HasCheckConstraint("CK_PurchaseOrderLines_CurrencyCode_Uppercase", "\"CurrencyCode\" = upper(\"CurrencyCode\")");
            });
            line.WithOwner().HasForeignKey("PurchaseOrderId");
            line.HasKey(purchaseOrderLine => purchaseOrderLine.Id);
            line.Property(purchaseOrderLine => purchaseOrderLine.Id).ValueGeneratedNever();
            line.Property(purchaseOrderLine => purchaseOrderLine.SupplierProductId).HasColumnType("uuid").IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.LineNumber).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.ProductId).HasColumnType("uuid").IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.ProductSku).HasMaxLength(PurchaseOrderRules.MaxProductSkuLength).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.ProductName).HasMaxLength(PurchaseOrderRules.MaxProductNameLength).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.SupplierSku).HasMaxLength(PurchaseOrderRules.MaxSupplierSkuLength);
            line.Property(purchaseOrderLine => purchaseOrderLine.PurchaseUnitOfMeasure).HasMaxLength(SupplierProductRules.UnitOfMeasureLength).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.Quantity).HasPrecision(18, 6).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.QuantityInBaseUnit).HasPrecision(18, 6).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.ConversionFactorToBaseUnit).HasPrecision(18, 6).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.UnitPrice).HasPrecision(18, 4).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.CurrencyCode).HasMaxLength(SupplierProductRules.CurrencyCodeLength).IsRequired();
            line.Property(purchaseOrderLine => purchaseOrderLine.LineAmount).HasPrecision(18, 4).IsRequired();
            line.HasIndex(purchaseOrderLine => new { purchaseOrderLine.SupplierProductId, purchaseOrderLine.PurchaseUnitOfMeasure });
            line.HasIndex("PurchaseOrderId", nameof(PurchaseOrderLine.LineNumber)).IsUnique();
            line.HasOne<Product>().WithMany().HasForeignKey(purchaseOrderLine => purchaseOrderLine.ProductId).OnDelete(DeleteBehavior.Restrict);
            line.HasOne<SupplierProduct>().WithMany().HasForeignKey(purchaseOrderLine => purchaseOrderLine.SupplierProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation(purchaseOrder => purchaseOrder.Lines).HasField("lines").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(purchaseOrder => purchaseOrder.StatusHistory, history =>
        {
            history.ToTable("PurchaseOrderStatusHistory");
            history.WithOwner().HasForeignKey("PurchaseOrderId");
            history.HasKey(item => item.Id);
            history.Property(item => item.Id).ValueGeneratedNever();
            history.Property(item => item.PreviousStatus).HasConversion<int?>();
            history.Property(item => item.Status).HasConversion<int>().IsRequired();
            history.Property(item => item.ChangedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
            history.Property(item => item.ActorUserId).HasColumnType("uuid").IsRequired();
            history.Property(item => item.Reason).HasMaxLength(PurchaseOrderRules.MaxStatusReasonLength);
        });
        builder.Navigation(purchaseOrder => purchaseOrder.StatusHistory).HasField("statusHistory").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
