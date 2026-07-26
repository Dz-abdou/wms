using Warehouse.Domain.Purchasing;

namespace Warehouse.UnitTests.Purchasing;

public sealed class PurchaseOrderTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 25, 20, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Submitted_order_is_immutable_and_line_snapshots_catalogue_terms()
    {
        var catalogueItem = SupplierProduct.Create(Guid.NewGuid(), Guid.NewGuid(), " ACME-CTN ", "CTN", 2m, 15m, "DZD", CreatedAtUtc);
        var purchaseOrder = PurchaseOrder.Create(catalogueItem.SupplierId, CreatedAtUtc);
        purchaseOrder.ReplaceLines([PurchaseOrderLine.Create(catalogueItem, "SKU-001", "Sample product", 2m)], CreatedAtUtc.AddMinutes(1));
        purchaseOrder.Submit(CreatedAtUtc.AddMinutes(2));

        var line = Assert.Single(purchaseOrder.Lines);
        Assert.Equal("ACME-CTN", line.SupplierSku);
        Assert.Equal("CTN", line.PurchaseUnitOfMeasure);
        Assert.Equal(15m, line.UnitPrice);
        Assert.Equal("DZD", line.CurrencyCode);
        Assert.Equal(PurchaseOrderStatus.Submitted, purchaseOrder.Status);
        Assert.Throws<InvalidOperationException>(() => purchaseOrder.ReplaceLines([], CreatedAtUtc.AddMinutes(3)));
    }

    [Fact]
    public void Submit_rejects_an_empty_draft()
    {
        var purchaseOrder = PurchaseOrder.Create(Guid.NewGuid(), CreatedAtUtc);

        Assert.Throws<InvalidOperationException>(() => purchaseOrder.Submit(CreatedAtUtc.AddMinutes(1)));
    }
}
