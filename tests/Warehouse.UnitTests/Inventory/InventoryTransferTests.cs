using Warehouse.Domain.Inventory;

namespace Warehouse.UnitTests.Inventory;

public sealed class InventoryTransferTests
{
    private static readonly DateTime TransferredAtUtc = new(2026, 7, 31, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_transfer_line_preserves_the_quantity_snapshot_and_links_movements()
    {
        var transferId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var line = InventoryTransferLine.Create(
            transferId,
            1,
            productId,
            "CTN",
            2m,
            48m,
            TransferredAtUtc,
            Guid.NewGuid());
        var transferOutMovementId = Guid.NewGuid();
        var transferInMovementId = Guid.NewGuid();

        line.LinkMovements(transferOutMovementId, transferInMovementId);

        Assert.Equal(transferId, line.InventoryTransferId);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal("CTN", line.UnitOfMeasure);
        Assert.Equal(2m, line.QuantityInUnit);
        Assert.Equal(48m, line.QuantityInBaseUnit);
        Assert.Equal(transferOutMovementId, line.TransferOutMovementId);
        Assert.Equal(transferInMovementId, line.TransferInMovementId);
    }

    [Fact]
    public void Create_transfer_movements_sets_opposite_signed_types()
    {
        var transferId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sourceWarehouseId = Guid.NewGuid();
        var destinationWarehouseId = Guid.NewGuid();

        var transferOut = InventoryMovement.CreateTransferOut(
            transferId,
            productId,
            sourceWarehouseId,
            "EA",
            3m,
            3m,
            7m,
            TransferredAtUtc);
        var transferIn = InventoryMovement.CreateTransferIn(
            transferId,
            productId,
            destinationWarehouseId,
            "EA",
            3m,
            3m,
            3m,
            TransferredAtUtc);

        Assert.Equal(InventoryMovementType.TransferOut, transferOut.Type);
        Assert.Equal(-3m, transferOut.QuantityDeltaInUnit);
        Assert.Equal(-3m, transferOut.QuantityDelta);
        Assert.Equal(InventoryMovementType.TransferIn, transferIn.Type);
        Assert.Equal(3m, transferIn.QuantityDeltaInUnit);
        Assert.Equal(3m, transferIn.QuantityDelta);
        Assert.Equal(transferId, transferOut.InventoryTransferId);
        Assert.Equal(transferId, transferIn.InventoryTransferId);
    }
}
