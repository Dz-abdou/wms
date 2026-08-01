using Warehouse.Domain.Customers;

namespace Warehouse.UnitTests.Customers;

public sealed class CustomerTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_normalizes_customer_fields_and_optional_currency()
    {
        var customer = Customer.Create(" customer-01 ", " Acme Distribution ", " Acme ", " eur ", " Deliver before noon ", null, CreatedAtUtc);

        Assert.Equal("CUSTOMER-01", customer.Code);
        Assert.Equal("Acme Distribution", customer.LegalName);
        Assert.Equal("Acme", customer.TradingName);
        Assert.Equal("EUR", customer.DefaultCurrencyCode);
        Assert.Equal("Deliver before noon", customer.DeliveryInstructions);
        Assert.True(customer.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_rejects_blank_customer_code(string? code)
    {
        Assert.Throws<ArgumentException>(() => Customer.Create(code, "Acme", null, null, null, null, CreatedAtUtc));
    }

    [Fact]
    public void Address_requires_a_shipping_or_billing_purpose()
    {
        var customer = Customer.Create("ACME", "Acme", null, null, null, null, CreatedAtUtc);

        Assert.Throws<ArgumentException>(() => CustomerAddress.Create(customer.Id, "Main", "1 Main St", null, "Algiers", null, "dz", false, false, null, CreatedAtUtc));
    }

    [Fact]
    public void Status_update_is_idempotent_and_preserves_actor_on_no_op()
    {
        var creatorId = Guid.NewGuid();
        var customer = Customer.Create("ACME", "Acme", null, null, null, null, CreatedAtUtc, creatorId);
        var statusActorId = Guid.NewGuid();
        var changedAtUtc = CreatedAtUtc.AddMinutes(1);
        customer.SetStatus(false, changedAtUtc, statusActorId);
        customer.SetStatus(false, changedAtUtc.AddMinutes(1), Guid.NewGuid());

        Assert.False(customer.IsActive);
        Assert.Equal(changedAtUtc, customer.UpdatedAtUtc);
        Assert.Equal(statusActorId, customer.UpdatedByUserId);
    }
}
