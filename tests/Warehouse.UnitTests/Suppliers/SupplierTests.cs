using Warehouse.Domain.Suppliers;

namespace Warehouse.UnitTests.Suppliers;

public sealed class SupplierTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 25, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_normalizes_code_and_optional_contact_fields()
    {
        var supplier = Supplier.Create(" supplier-01 ", " Acme supplies ", " sales@acme.test ", " +213 555 000 000 ", " 1 Main Street ", CreatedAtUtc);

        Assert.Equal("SUPPLIER-01", supplier.Code);
        Assert.Equal("Acme supplies", supplier.Name);
        Assert.Equal("sales@acme.test", supplier.Email);
        Assert.Equal("+213 555 000 000", supplier.PhoneNumber);
        Assert.Equal("1 Main Street", supplier.Address);
        Assert.True(supplier.IsActive);
        Assert.Equal(SupplierRules.DefaultCurrencyCode, supplier.DefaultCurrencyCode);
    }

    [Fact]
    public void Create_normalizes_default_currency()
    {
        var supplier = Supplier.Create("SUPPLIER-01", "Acme supplies", null, null, null, CreatedAtUtc, defaultCurrencyCode: " eur ");

        Assert.Equal("EUR", supplier.DefaultCurrencyCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_code(string? code)
    {
        Assert.Throws<ArgumentException>(() => Supplier.Create(code, "Acme", null, null, null, CreatedAtUtc));
    }

    [Fact]
    public void Status_update_is_idempotent_and_preserves_actor_on_no_op()
    {
        var creatorId = Guid.NewGuid();
        var supplier = Supplier.Create("ACME", "Acme", null, null, null, CreatedAtUtc, creatorId);
        var statusActorId = Guid.NewGuid();
        var changedAtUtc = CreatedAtUtc.AddMinutes(1);
        supplier.SetStatus(false, changedAtUtc, statusActorId);
        supplier.SetStatus(false, changedAtUtc.AddMinutes(1), Guid.NewGuid());

        Assert.False(supplier.IsActive);
        Assert.Equal(changedAtUtc, supplier.UpdatedAtUtc);
        Assert.Equal(statusActorId, supplier.UpdatedByUserId);
    }

    [Fact]
    public void Create_rejects_values_above_supported_lengths()
    {
        Assert.Throws<ArgumentException>(() => Supplier.Create(new string('S', SupplierRules.MaxCodeLength + 1), "Acme", null, null, null, CreatedAtUtc));
        Assert.Throws<ArgumentException>(() => Supplier.Create("ACME", new string('S', SupplierRules.MaxNameLength + 1), null, null, null, CreatedAtUtc));
    }
}
