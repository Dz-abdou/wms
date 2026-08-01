using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Customers;

namespace Warehouse.Infrastructure.Customers;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", table =>
        {
            table.HasCheckConstraint("CK_Customers_Code_NotBlank", "btrim(\"Code\") <> ''");
            table.HasCheckConstraint("CK_Customers_LegalName_NotBlank", "btrim(\"LegalName\") <> ''");
            table.HasCheckConstraint("CK_Customers_Code_Uppercase", "\"Code\" = upper(\"Code\")");
            table.HasCheckConstraint("CK_Customers_DefaultCurrencyCode_Uppercase", "\"DefaultCurrencyCode\" IS NULL OR \"DefaultCurrencyCode\" = upper(\"DefaultCurrencyCode\")");
        });
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Code).HasMaxLength(CustomerRules.MaxCodeLength).IsRequired();
        builder.Property(customer => customer.LegalName).HasMaxLength(CustomerRules.MaxLegalNameLength).IsRequired();
        builder.Property(customer => customer.TradingName).HasMaxLength(CustomerRules.MaxTradingNameLength);
        builder.Property(customer => customer.DefaultCurrencyCode).HasMaxLength(CustomerRules.CurrencyCodeLength);
        builder.Property(customer => customer.DeliveryInstructions).HasMaxLength(CustomerRules.MaxDeliveryInstructionsLength);
        builder.Property(customer => customer.ServiceNotes).HasMaxLength(CustomerRules.MaxServiceNotesLength);
        builder.Property(customer => customer.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(customer => customer.CreatedByUserId).HasColumnType("uuid");
        builder.Property(customer => customer.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(customer => customer.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(customer => customer.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(customer => customer.Code).IsUnique().HasDatabaseName("UX_Customers_Code");
        builder.HasIndex(customer => customer.LegalName).HasDatabaseName("IX_Customers_LegalName");
    }
}
