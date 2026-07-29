using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Receiving;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Infrastructure.Receiving;

public sealed class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");
        builder.HasKey(receipt => receipt.Id);
        builder.Property(receipt => receipt.Number).HasMaxLength(32).IsRequired();
        builder.Property(receipt => receipt.PurchaseOrderId).HasColumnType("uuid").IsRequired();
        builder.Property(receipt => receipt.WarehouseId).HasColumnType("uuid").IsRequired();
        builder.Property(receipt => receipt.ReceivedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(receipt => receipt.SupplierDeliveryNote).HasMaxLength(128);
        builder.Property(receipt => receipt.Notes).HasMaxLength(2000);
        builder.Property(receipt => receipt.ReceiverUserId).HasColumnType("uuid").IsRequired();
        builder.HasIndex(receipt => receipt.Number).IsUnique();
        builder.HasIndex(receipt => new { receipt.PurchaseOrderId, receipt.ReceivedAtUtc });
        builder.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(receipt => receipt.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>()
            .WithMany()
            .HasForeignKey(receipt => receipt.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.OwnsMany(receipt => receipt.Lines, line =>
        {
            line.ToTable("GoodsReceiptLines", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_GoodsReceiptLines_AcceptedQuantity_Positive", "\"AcceptedQuantity\" > 0");
                tableBuilder.HasCheckConstraint("CK_GoodsReceiptLines_AcceptedQuantityInBaseUnit_Positive", "\"AcceptedQuantityInBaseUnit\" > 0");
            });
            line.WithOwner().HasForeignKey("GoodsReceiptId"); line.HasKey(item => item.Id);
            line.Property(item => item.PurchaseOrderLineId).HasColumnType("uuid").IsRequired(); line.Property(item => item.PurchaseOrderLineNumber).IsRequired(); line.Property(item => item.ProductId).HasColumnType("uuid").IsRequired();
            line.Property(item => item.ProductSku).HasMaxLength(64).IsRequired(); line.Property(item => item.ProductName).HasMaxLength(256).IsRequired(); line.Property(item => item.UnitOfMeasure).HasMaxLength(16).IsRequired();
            line.Property(item => item.AcceptedQuantity).HasPrecision(18, 6).IsRequired(); line.Property(item => item.AcceptedQuantityInBaseUnit).HasPrecision(18, 6).IsRequired(); line.Property(item => item.ConversionFactorToBaseUnit).HasPrecision(18, 6).IsRequired(); line.Property(item => item.InventoryMovementId).HasColumnType("uuid").IsRequired();
            line.HasIndex("GoodsReceiptId", nameof(GoodsReceiptLine.PurchaseOrderLineId)).IsUnique();
            line.HasIndex(item => item.InventoryMovementId).IsUnique();
            line.HasOne<InventoryMovement>()
                .WithMany()
                .HasForeignKey(item => item.InventoryMovementId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Navigation(receipt => receipt.Lines).HasField("lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
