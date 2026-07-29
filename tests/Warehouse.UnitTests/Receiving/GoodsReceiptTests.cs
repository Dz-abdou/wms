using Warehouse.Domain.Receiving;

namespace Warehouse.UnitTests.Receiving;

public sealed class GoodsReceiptTests
{
    private static readonly DateTime ReceivedAtUtc = new(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Receipt_rejects_duplicate_purchase_order_lines()
    {
        var receipt = GoodsReceipt.Create(
            "GR-2026-000001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            ReceivedAtUtc,
            null,
            null,
            Guid.NewGuid());
        var purchaseOrderLineId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => receipt.AddLines([
            GoodsReceiptLine.Create(purchaseOrderLineId, 1, productId, "SKU-001", "Product", "EA", 1m, 1m, Guid.NewGuid()),
            GoodsReceiptLine.Create(purchaseOrderLineId, 1, productId, "SKU-001", "Product", "EA", 1m, 1m, Guid.NewGuid())
        ]));
    }

    [Fact]
    public void Receipt_line_snapshots_the_converted_base_quantity()
    {
        var line = GoodsReceiptLine.Create(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            "SKU-001",
            "Product",
            "CTN",
            2m,
            12m,
            Guid.NewGuid());

        Assert.Equal(24m, line.AcceptedQuantityInBaseUnit);
        Assert.Equal("CTN", line.UnitOfMeasure);
    }
}
