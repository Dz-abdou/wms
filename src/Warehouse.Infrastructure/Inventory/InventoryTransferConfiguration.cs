using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Infrastructure.Inventory;

public sealed class InventoryTransferConfiguration : IEntityTypeConfiguration<InventoryTransfer>
{
    public void Configure(EntityTypeBuilder<InventoryTransfer> builder)
    {
        builder.ToTable("InventoryTransfers", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_InventoryTransfers_DifferentWarehouses",
                "\"SourceWarehouseId\" <> \"DestinationWarehouseId\"");
        });
        builder.HasKey(transfer => transfer.Id);
        builder.Property(transfer => transfer.Number).HasMaxLength(InventoryTransferRules.MaxNumberLength).HasDefaultValue(string.Empty).IsRequired();
        builder.Property(transfer => transfer.SourceWarehouseId).HasColumnType("uuid").IsRequired();
        builder.Property(transfer => transfer.DestinationWarehouseId).HasColumnType("uuid").IsRequired();
        builder.Property(transfer => transfer.Reference).HasMaxLength(InventoryTransferRules.MaxReferenceLength);
        builder.Property(transfer => transfer.Note).HasMaxLength(InventoryTransferRules.MaxNoteLength);
        builder.Property(transfer => transfer.TransferredAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(transfer => transfer.CreatedByUserId).HasColumnType("uuid");
        builder.Property(transfer => transfer.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(transfer => transfer.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(transfer => transfer.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(transfer => new { transfer.SourceWarehouseId, transfer.TransferredAtUtc });
        builder.HasIndex(transfer => transfer.Number).IsUnique().HasFilter("\"Number\" <> ''");
        builder.HasIndex(transfer => new { transfer.DestinationWarehouseId, transfer.TransferredAtUtc });
        builder.HasOne<WarehouseEntity>()
            .WithMany()
            .HasForeignKey(transfer => transfer.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>()
            .WithMany()
            .HasForeignKey(transfer => transfer.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class InventoryTransferLineConfiguration : IEntityTypeConfiguration<InventoryTransferLine>
{
    public void Configure(EntityTypeBuilder<InventoryTransferLine> builder)
    {
        builder.ToTable("InventoryTransferLines", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_InventoryTransferLines_LineNumber_Positive", "\"LineNumber\" > 0");
            tableBuilder.HasCheckConstraint("CK_InventoryTransferLines_QuantityInUnit_Positive", "\"QuantityInUnit\" > 0");
            tableBuilder.HasCheckConstraint("CK_InventoryTransferLines_QuantityInBaseUnit_Positive", "\"QuantityInBaseUnit\" > 0");
        });
        builder.HasKey(line => line.Id);
        builder.Property(line => line.InventoryTransferId).HasColumnType("uuid").IsRequired();
        builder.Property(line => line.ProductId).HasColumnType("uuid").IsRequired();
        builder.Property(line => line.LineNumber).IsRequired();
        builder.Property(line => line.UnitOfMeasure).HasMaxLength(ProductUnitOfMeasure.MaxCodeLength).IsRequired();
        builder.Property(line => line.QuantityInUnit).HasPrecision(18, 3).IsRequired();
        builder.Property(line => line.QuantityInBaseUnit).HasPrecision(18, 3).IsRequired();
        builder.Property(line => line.TransferOutMovementId).HasColumnType("uuid");
        builder.Property(line => line.TransferInMovementId).HasColumnType("uuid");
        builder.Property(line => line.CreatedByUserId).HasColumnType("uuid");
        builder.Property(line => line.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(line => line.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(line => line.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(line => new { line.InventoryTransferId, line.LineNumber }).IsUnique();
        builder.HasIndex(line => new { line.InventoryTransferId, line.ProductId }).IsUnique();
        builder.HasIndex(line => line.TransferOutMovementId).IsUnique().HasFilter("\"TransferOutMovementId\" IS NOT NULL");
        builder.HasIndex(line => line.TransferInMovementId).IsUnique().HasFilter("\"TransferInMovementId\" IS NOT NULL");
        builder.HasOne<InventoryTransfer>()
            .WithMany()
            .HasForeignKey(line => line.InventoryTransferId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(line => line.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InventoryMovement>()
            .WithMany()
            .HasForeignKey(line => line.TransferOutMovementId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InventoryMovement>()
            .WithMany()
            .HasForeignKey(line => line.TransferInMovementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
