using Warehouse.Domain.Products;
using Warehouse.Domain.Sales;

namespace Warehouse.UnitTests.Sales;

public sealed class SalesOrderTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 8, 1, 19, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Submitted_order_is_immutable_and_preserves_product_snapshot()
    {
        var actorId = Guid.NewGuid();
        var product = Product.Create("SKU-001", "Sample product", null, "EA", [], null, CreatedAtUtc, actorId);
        var order = SalesOrder.Create("SO-2026-000001", Guid.NewGuid(), "CUSTOMER-01", "Acme", Guid.NewGuid(), Guid.NewGuid(), "MAIN", "Main warehouse", new SalesOrderShippingAddress("Main", "1 Main St", null, "Algiers", null, "DZ", null), "dzd", new DateOnly(2026, 8, 1), null, null, null, actorId, CreatedAtUtc);

        order.ReplaceLines([SalesOrderLine.Create(1, product, "EA", 2m)], CreatedAtUtc.AddMinutes(1), actorId);
        order.Submit(CreatedAtUtc.AddMinutes(2), actorId);

        var line = Assert.Single(order.Lines);
        Assert.Equal("SKU-001", line.ProductSku);
        Assert.Equal("Sample product", line.ProductName);
        Assert.Equal("EA", line.UnitOfMeasure);
        Assert.Equal(2m, line.QuantityInBaseUnit);
        Assert.Equal("DZD", order.CurrencyCode);
        Assert.Equal("MAIN", order.FulfillmentWarehouseCode);
        Assert.Equal(SalesOrderStatus.Submitted, order.Status);
        Assert.Throws<InvalidOperationException>(() => order.ReplaceLines([], CreatedAtUtc.AddMinutes(3), actorId));
    }

    [Fact]
    public void Submit_rejects_empty_draft()
    {
        var actorId = Guid.NewGuid();
        var order = SalesOrder.Create("SO-2026-000001", Guid.NewGuid(), "CUSTOMER-01", "Acme", Guid.NewGuid(), Guid.NewGuid(), "MAIN", "Main warehouse", new SalesOrderShippingAddress("Main", "1 Main St", null, "Algiers", null, "DZ", null), "DZD", new DateOnly(2026, 8, 1), null, null, null, actorId, CreatedAtUtc);

        Assert.Throws<InvalidOperationException>(() => order.Submit(CreatedAtUtc.AddMinutes(1), actorId));
    }
}
