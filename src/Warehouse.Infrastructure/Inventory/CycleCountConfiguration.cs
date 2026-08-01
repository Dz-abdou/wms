using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Products;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Infrastructure.Inventory;

public sealed class CycleCountConfiguration : IEntityTypeConfiguration<CycleCount>
{
    public void Configure(EntityTypeBuilder<CycleCount> builder)
    {
        builder.ToTable("CycleCounts");
        builder.HasKey(count => count.Id);
        builder.Property(count => count.Number).HasMaxLength(CycleCountRules.MaxNumberLength).HasDefaultValue(string.Empty).IsRequired();
        builder.Property(count => count.WarehouseId).HasColumnType("uuid").IsRequired();
        builder.Property(count => count.Reference).HasMaxLength(CycleCountRules.MaxReferenceLength);
        builder.Property(count => count.Note).HasMaxLength(CycleCountRules.MaxNoteLength);
        builder.Property(count => count.CountedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(count => count.CreatedByUserId).HasColumnType("uuid");
        builder.Property(count => count.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(count => count.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(count => count.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(count => new { count.WarehouseId, count.CountedAtUtc });
        builder.HasIndex(count => count.Number).IsUnique().HasFilter("\"Number\" <> ''");
        builder.HasOne<WarehouseEntity>()
            .WithMany()
            .HasForeignKey(count => count.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CycleCountLineConfiguration : IEntityTypeConfiguration<CycleCountLine>
{
    public void Configure(EntityTypeBuilder<CycleCountLine> builder)
    {
        builder.ToTable("CycleCountLines", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_CycleCountLines_LineNumber_Positive", "\"LineNumber\" > 0");
            tableBuilder.HasCheckConstraint("CK_CycleCountLines_SystemQuantity_NonNegative", "\"SystemQuantityInBase\" >= 0");
            tableBuilder.HasCheckConstraint("CK_CycleCountLines_CountedQuantityInUnit_NonNegative", "\"CountedQuantityInUnit\" >= 0");
            tableBuilder.HasCheckConstraint("CK_CycleCountLines_CountedQuantityInBase_NonNegative", "\"CountedQuantityInBase\" >= 0");
            tableBuilder.HasCheckConstraint("CK_CycleCountLines_SystemBalanceVersion_NonNegative", "\"SystemBalanceVersion\" >= 0");
        });
        builder.HasKey(line => line.Id);
        builder.Property(line => line.CycleCountId).HasColumnType("uuid").IsRequired();
        builder.Property(line => line.ProductId).HasColumnType("uuid").IsRequired();
        builder.Property(line => line.LineNumber).IsRequired();
        builder.Property(line => line.SystemQuantityInBase).HasPrecision(18, 3).IsRequired();
        builder.Property(line => line.SystemBalanceVersion).IsRequired();
        builder.Property(line => line.CountedUnitOfMeasure).HasMaxLength(ProductUnitOfMeasure.MaxCodeLength).IsRequired();
        builder.Property(line => line.CountedQuantityInUnit).HasPrecision(18, 3).IsRequired();
        builder.Property(line => line.CountedQuantityInBase).HasPrecision(18, 3).IsRequired();
        builder.Property(line => line.VarianceQuantityInBase).HasPrecision(18, 3).IsRequired();
        builder.Property(line => line.InventoryMovementId).HasColumnType("uuid");
        builder.Property(line => line.CreatedByUserId).HasColumnType("uuid");
        builder.Property(line => line.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(line => line.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(line => line.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(line => new { line.CycleCountId, line.LineNumber }).IsUnique();
        builder.HasIndex(line => new { line.CycleCountId, line.ProductId }).IsUnique();
        builder.HasIndex(line => line.InventoryMovementId).IsUnique().HasFilter("\"InventoryMovementId\" IS NOT NULL");
        builder.HasOne<CycleCount>()
            .WithMany()
            .HasForeignKey(line => line.CycleCountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(line => line.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InventoryMovement>()
            .WithMany()
            .HasForeignKey(line => line.InventoryMovementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
