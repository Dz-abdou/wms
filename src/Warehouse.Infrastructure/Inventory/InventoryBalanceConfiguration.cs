using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Inventory;

namespace Warehouse.Infrastructure.Inventory;

public sealed class InventoryBalanceConfiguration : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        builder.ToTable("InventoryBalances", tableBuilder =>
            tableBuilder.HasCheckConstraint("CK_InventoryBalances_Quantity_NonNegative", "\"Quantity\" >= 0"));
        builder.HasKey(balance => balance.Id);
        builder.Property(balance => balance.ProductId).HasColumnType("uuid").IsRequired();
        builder.Property(balance => balance.WarehouseId).HasColumnType("uuid").IsRequired();
        builder.Property(balance => balance.Quantity).HasPrecision(18, 3).IsRequired();
        builder.Property(balance => balance.Version).IsConcurrencyToken().IsRequired();
        builder.Property(balance => balance.CreatedByUserId).HasColumnType("uuid");
        builder.Property(balance => balance.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(balance => balance.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(balance => balance.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(balance => new { balance.ProductId, balance.WarehouseId }).IsUnique();
    }
}
