using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Customers;

namespace Warehouse.Infrastructure.Customers;

public sealed class CustomerContactConfiguration : IEntityTypeConfiguration<CustomerContact>
{
    public void Configure(EntityTypeBuilder<CustomerContact> builder)
    {
        builder.ToTable("CustomerContacts", table =>
        {
            table.HasCheckConstraint("CK_CustomerContacts_Name_NotBlank", "btrim(\"Name\") <> ''");
        });
        builder.HasKey(contact => contact.Id);
        builder.Property(contact => contact.CustomerId).HasColumnType("uuid").IsRequired();
        builder.Property(contact => contact.Name).HasMaxLength(CustomerContactRules.MaxNameLength).IsRequired();
        builder.Property(contact => contact.Role).HasMaxLength(CustomerContactRules.MaxRoleLength);
        builder.Property(contact => contact.Email).HasMaxLength(CustomerContactRules.MaxEmailLength);
        builder.Property(contact => contact.PhoneNumber).HasMaxLength(CustomerContactRules.MaxPhoneNumberLength);
        builder.Property(contact => contact.CreatedByUserId).HasColumnType("uuid");
        builder.Property(contact => contact.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(contact => contact.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(contact => contact.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(contact => contact.CustomerId);
        builder.HasOne<Customer>().WithMany().HasForeignKey(contact => contact.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
