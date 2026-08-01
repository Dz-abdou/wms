using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Customers;
using Warehouse.Domain.Products;
using Warehouse.Domain.Sales;
using WarehouseEntity = Warehouse.Domain.Warehouses.Warehouse;

namespace Warehouse.Infrastructure.Sales;

public sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders", table =>
        {
            table.HasCheckConstraint("CK_SalesOrders_Status_Valid", "\"Status\" IN (0, 1, 2)");
            table.HasCheckConstraint("CK_SalesOrders_Number_NotBlank", "btrim(\"Number\") <> ''");
            table.HasCheckConstraint("CK_SalesOrders_CurrencyCode_Uppercase", "\"CurrencyCode\" = upper(\"CurrencyCode\")");
        });
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Number).HasMaxLength(SalesOrderRules.MaxNumberLength).IsRequired();
        builder.Property(order => order.CustomerId).HasColumnType("uuid").IsRequired();
        builder.Property(order => order.CustomerCode).HasMaxLength(SalesOrderRules.MaxCustomerCodeLength).IsRequired();
        builder.Property(order => order.CustomerName).HasMaxLength(SalesOrderRules.MaxCustomerNameLength).IsRequired();
        builder.Property(order => order.ShippingAddressId).HasColumnType("uuid").IsRequired();
        builder.Property(order => order.FulfillmentWarehouseId).HasColumnType("uuid").IsRequired();
        builder.Property(order => order.FulfillmentWarehouseCode).HasMaxLength(SalesOrderRules.MaxWarehouseCodeLength).IsRequired();
        builder.Property(order => order.FulfillmentWarehouseName).HasMaxLength(SalesOrderRules.MaxWarehouseNameLength).IsRequired();
        builder.Property(order => order.CurrencyCode).HasMaxLength(SalesOrderRules.CurrencyCodeLength).IsRequired();
        builder.Property(order => order.OrderDate).HasColumnType("date").IsRequired();
        builder.Property(order => order.RequestedShipDate).HasColumnType("date");
        builder.Property(order => order.CustomerReference).HasMaxLength(SalesOrderRules.MaxCustomerReferenceLength);
        builder.Property(order => order.DeliveryInstructions).HasMaxLength(SalesOrderRules.MaxDeliveryInstructionsLength);
        builder.Property(order => order.OwnerUserId).HasColumnType("uuid").IsRequired();
        builder.Property(order => order.SubmittedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(order => order.Version).IsConcurrencyToken().IsRequired();
        builder.Property(order => order.Status).HasConversion<int>().IsRequired();
        builder.Property(order => order.CreatedByUserId).HasColumnType("uuid");
        builder.Property(order => order.UpdatedByUserId).HasColumnType("uuid");
        builder.Property(order => order.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(order => order.UpdatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(order => order.Number).IsUnique();
        builder.HasIndex(order => new { order.CustomerId, order.Status });
        builder.HasIndex(order => new { order.FulfillmentWarehouseId, order.Status });
        builder.HasOne<Customer>().WithMany().HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CustomerAddress>().WithMany().HasForeignKey(order => order.ShippingAddressId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(order => order.FulfillmentWarehouseId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(order => order.ShippingAddress, address =>
        {
            address.Property(item => item.Label).HasColumnName("ShippingAddressLabel").HasMaxLength(CustomerAddressRules.MaxLabelLength).IsRequired();
            address.Property(item => item.AddressLine1).HasColumnName("ShippingAddressLine1").HasMaxLength(CustomerAddressRules.MaxAddressLineLength).IsRequired();
            address.Property(item => item.AddressLine2).HasColumnName("ShippingAddressLine2").HasMaxLength(CustomerAddressRules.MaxAddressLineLength);
            address.Property(item => item.City).HasColumnName("ShippingAddressCity").HasMaxLength(CustomerAddressRules.MaxCityLength).IsRequired();
            address.Property(item => item.PostalCode).HasColumnName("ShippingAddressPostalCode").HasMaxLength(CustomerAddressRules.MaxPostalCodeLength);
            address.Property(item => item.CountryCode).HasColumnName("ShippingAddressCountryCode").HasMaxLength(CustomerAddressRules.CountryCodeLength).IsRequired();
            address.Property(item => item.DeliveryInstructions).HasColumnName("ShippingAddressInstructions").HasMaxLength(CustomerAddressRules.MaxDeliveryInstructionsLength);
        });

        builder.OwnsMany(order => order.Lines, line =>
        {
            line.ToTable("SalesOrderLines", table => table.HasCheckConstraint("CK_SalesOrderLines_Quantity_Positive", "\"Quantity\" > 0"));
            line.WithOwner().HasForeignKey("SalesOrderId");
            line.HasKey(item => item.Id);
            line.Property(item => item.Id).ValueGeneratedNever();
            line.Property(item => item.LineNumber).IsRequired();
            line.Property(item => item.ProductId).HasColumnType("uuid").IsRequired();
            line.Property(item => item.ProductSku).HasMaxLength(ProductRules.MaxSkuLength).IsRequired();
            line.Property(item => item.ProductName).HasMaxLength(ProductRules.MaxNameLength).IsRequired();
            line.Property(item => item.UnitOfMeasure).HasMaxLength(ProductUnitOfMeasure.MaxCodeLength).IsRequired();
            line.Property(item => item.Quantity).HasPrecision(18, 6).IsRequired();
            line.Property(item => item.QuantityInBaseUnit).HasPrecision(18, 6).IsRequired();
            line.Property(item => item.ConversionFactorToBaseUnit).HasPrecision(18, 6).IsRequired();
            line.HasIndex("SalesOrderId", nameof(SalesOrderLine.LineNumber)).IsUnique();
            line.HasOne<Product>().WithMany().HasForeignKey(item => item.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Navigation(order => order.Lines).HasField("lines").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(order => order.StatusHistory, history =>
        {
            history.ToTable("SalesOrderStatusHistory");
            history.WithOwner().HasForeignKey("SalesOrderId");
            history.HasKey(item => item.Id);
            history.Property(item => item.Id).ValueGeneratedNever();
            history.Property(item => item.PreviousStatus).HasConversion<int?>();
            history.Property(item => item.Status).HasConversion<int>().IsRequired();
            history.Property(item => item.ChangedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
            history.Property(item => item.ActorUserId).HasColumnType("uuid").IsRequired();
            history.Property(item => item.Reason).HasMaxLength(SalesOrderRules.MaxStatusReasonLength);
        });
        builder.Navigation(order => order.StatusHistory).HasField("statusHistory").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
