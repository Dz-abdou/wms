using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Customers;

namespace Warehouse.Infrastructure.Customers;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses", table =>
        {
            table.HasCheckConstraint("CK_CustomerAddresses_Label_NotBlank", "btrim(\"Label\") <> ''");
            table.HasCheckConstraint("CK_CustomerAddresses_AddressLine1_NotBlank", "btrim(\"AddressLine1\") <> ''");
            table.HasCheckConstraint("CK_CustomerAddresses_City_NotBlank", "btrim(\"City\") <> ''");
            table.HasCheckConstraint("CK_CustomerAddresses_CountryCode_Uppercase", "\"CountryCode\" = upper(\"CountryCode\")");
            table.HasCheckConstraint("CK_CustomerAddresses_HasPurpose", "\"IsShippingAddress\" OR \"IsBillingAddress\"");
        });
        builder.HasKey(address => address.Id);
        builder.Property(address => address.CustomerId).HasColumnType("uuid").IsRequired();
        builder.Property(address => address.Label).HasMaxLength(CustomerAddressRules.MaxLabelLength).IsRequired();
        builder.Property(address => address.AddressLine1).HasMaxLength(CustomerAddressRules.MaxAddressLineLength).IsRequired();
        builder.Property(address => address.AddressLine2).HasMaxLength(CustomerAddressRules.MaxAddressLineLength);
        builder.Property(address => address.City).HasMaxLength(CustomerAddressRules.MaxCityLength).IsRequired();
        builder.Property(address => address.PostalCode).HasMaxLength(CustomerAddressRules.MaxPostalCodeLength);
        builder.Property(address => address.CountryCode).HasMaxLength(CustomerAddressRules.CountryCodeLength).IsRequired();
        builder.Property(address => address.DeliveryInstructions).HasMaxLength(CustomerAddressRules.MaxDeliveryInstructionsLength);
        builder.Property(address => address.CreatedByUserId).HasColumnType("uuid");
        builder.Property(address => address.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(address => address.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(address => address.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(address => address.CustomerId);
        builder.HasOne<Customer>().WithMany().HasForeignKey(address => address.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
