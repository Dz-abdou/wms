using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Inventory;

namespace Warehouse.Infrastructure.Inventory;

public sealed class InventoryAdjustmentConfiguration : IEntityTypeConfiguration<InventoryAdjustment>
{
    public void Configure(EntityTypeBuilder<InventoryAdjustment> builder)
    {
        builder.ToTable("InventoryAdjustments");
        builder.HasKey(adjustment => adjustment.Id);
        builder.Property(adjustment => adjustment.Reason).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(adjustment => adjustment.Reference).HasMaxLength(InventoryAdjustmentRules.MaxReferenceLength);
        builder.Property(adjustment => adjustment.Note).HasMaxLength(InventoryAdjustmentRules.MaxNoteLength);
        builder.Property(adjustment => adjustment.CreatedByUserId).HasColumnType("uuid");
        builder.Property(adjustment => adjustment.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(adjustment => adjustment.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(adjustment => adjustment.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(adjustment => adjustment.CreatedAtUtc);
    }
}
