using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Suppliers;

namespace Warehouse.Infrastructure.Suppliers;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers", table =>
        {
            table.HasCheckConstraint("CK_Suppliers_Code_NotBlank", "btrim(\"Code\") <> ''");
            table.HasCheckConstraint("CK_Suppliers_Name_NotBlank", "btrim(\"Name\") <> ''");
            table.HasCheckConstraint("CK_Suppliers_Code_Uppercase", "\"Code\" = upper(\"Code\")");
        });
        builder.HasKey(supplier => supplier.Id);
        builder.Property(supplier => supplier.Code).HasMaxLength(SupplierRules.MaxCodeLength).IsRequired();
        builder.Property(supplier => supplier.Name).HasMaxLength(SupplierRules.MaxNameLength).IsRequired();
        builder.Property(supplier => supplier.Email).HasMaxLength(SupplierRules.MaxEmailLength);
        builder.Property(supplier => supplier.PhoneNumber).HasMaxLength(SupplierRules.MaxPhoneNumberLength);
        builder.Property(supplier => supplier.Address).HasMaxLength(SupplierRules.MaxAddressLength);
        builder.Property(supplier => supplier.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(supplier => supplier.CreatedByUserId).HasColumnType("uuid");
        builder.Property(supplier => supplier.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(supplier => supplier.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(supplier => supplier.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(supplier => supplier.Code).IsUnique().HasDatabaseName("UX_Suppliers_Code");
        builder.HasIndex(supplier => supplier.Name).HasDatabaseName("IX_Suppliers_Name");
    }
}
