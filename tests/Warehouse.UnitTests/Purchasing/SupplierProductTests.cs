using Warehouse.Domain.Purchasing;

namespace Warehouse.UnitTests.Purchasing;

public sealed class SupplierProductTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 25, 20, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_normalizes_catalogue_terms()
    {
        var catalogueItem = SupplierProduct.Create(Guid.NewGuid(), Guid.NewGuid(), " acme-24 ", " ctn ", 2m, 12.5m, " dzd ", CreatedAtUtc);

        Assert.Equal("acme-24", catalogueItem.SupplierSku);
        Assert.Equal("CTN", catalogueItem.PurchaseUnitOfMeasure);
        Assert.Equal(2m, catalogueItem.MinimumOrderQuantity);
        Assert.Equal(12.5m, catalogueItem.UnitPrice);
        Assert.Equal("DZD", catalogueItem.CurrencyCode);
        Assert.True(catalogueItem.IsActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_nonpositive_minimum_order_quantity(decimal quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SupplierProduct.Create(Guid.NewGuid(), Guid.NewGuid(), null, "EA", quantity, 1m, "DZD", CreatedAtUtc));
    }

    [Fact]
    public void Create_rejects_invalid_currency_code()
    {
        Assert.Throws<ArgumentException>(() => SupplierProduct.Create(Guid.NewGuid(), Guid.NewGuid(), null, "EA", 1m, 1m, "DZ", CreatedAtUtc));
    }
}
