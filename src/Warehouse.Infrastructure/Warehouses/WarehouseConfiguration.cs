using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Warehouses;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Infrastructure.Warehouses;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<WarehouseEntity>
{
    private const string CodeNotBlankConstraint = "CK_Warehouses_Code_NotBlank";
    private const string NameNotBlankConstraint = "CK_Warehouses_Name_NotBlank";
    private const string CodeUppercaseConstraint = "CK_Warehouses_Code_Uppercase";
    private const string CodeUniqueIndex = "UX_Warehouses_Code";

    public void Configure(EntityTypeBuilder<WarehouseEntity> builder)
    {
        builder.ToTable("Warehouses", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(CodeNotBlankConstraint, "btrim(\"Code\") <> ''");
            tableBuilder.HasCheckConstraint(NameNotBlankConstraint, "btrim(\"Name\") <> ''");
            tableBuilder.HasCheckConstraint(CodeUppercaseConstraint, "\"Code\" = upper(\"Code\")");
        });

        builder.HasKey(warehouse => warehouse.Id);
        builder.Property(warehouse => warehouse.Code).HasMaxLength(WarehouseRules.MaxCodeLength).IsRequired();
        builder.Property(warehouse => warehouse.Name).HasMaxLength(WarehouseRules.MaxNameLength).IsRequired();
        builder.Property(warehouse => warehouse.Description).HasMaxLength(WarehouseRules.MaxDescriptionLength);
        builder.Property(warehouse => warehouse.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(warehouse => warehouse.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(warehouse => warehouse.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(warehouse => warehouse.Code).IsUnique().HasDatabaseName(CodeUniqueIndex);
        builder.HasIndex(warehouse => warehouse.Name).HasDatabaseName("IX_Warehouses_Name");
    }
}