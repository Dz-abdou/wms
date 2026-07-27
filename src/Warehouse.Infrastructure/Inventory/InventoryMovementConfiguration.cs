using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;
using Warehouse.Domain.Receiving;

namespace Warehouse.Infrastructure.Inventory;

public sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_InventoryMovements_QuantityDelta_NonZero", "\"QuantityDelta\" <> 0");
            tableBuilder.HasCheckConstraint("CK_InventoryMovements_QuantityDeltaInUnit_NonZero", "\"QuantityDeltaInUnit\" <> 0");
        });
        builder.HasKey(movement => movement.Id);
        builder.Property(movement => movement.InventoryAdjustmentId).HasColumnType("uuid");
        builder.Property(movement => movement.GoodsReceiptId).HasColumnType("uuid");
        builder.Property(movement => movement.ProductId).HasColumnType("uuid").IsRequired();
        builder.Property(movement => movement.WarehouseId).HasColumnType("uuid").IsRequired();
        builder.Property(movement => movement.UnitOfMeasure).HasMaxLength(ProductUnitOfMeasure.MaxCodeLength).IsRequired();
        builder.Property(movement => movement.QuantityDeltaInUnit).HasPrecision(18, 3).IsRequired();
        builder.Property(movement => movement.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(movement => movement.QuantityDelta).HasPrecision(18, 3).IsRequired();
        builder.Property(movement => movement.BalanceAfter).HasPrecision(18, 3).IsRequired();
        builder.Property(movement => movement.CreatedByUserId).HasColumnType("uuid");
        builder.Property(movement => movement.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(movement => movement.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(movement => movement.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(movement => new { movement.ProductId, movement.WarehouseId, movement.CreatedAtUtc });
        builder.HasIndex(movement => movement.InventoryAdjustmentId);
        builder.HasIndex(movement => movement.GoodsReceiptId);
        builder.HasOne<InventoryAdjustment>()
            .WithMany()
            .HasForeignKey(movement => movement.InventoryAdjustmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GoodsReceipt>()
            .WithMany()
            .HasForeignKey(movement => movement.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
