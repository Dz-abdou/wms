using Warehouse.Domain.Inventory;

namespace Warehouse.UnitTests.Inventory;

public sealed class InventoryBalanceTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 22, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Apply_adjustment_updates_the_balance_metadata_and_version()
    {
        var creatorId = Guid.NewGuid();
        var balance = InventoryBalance.Create(Guid.NewGuid(), Guid.NewGuid(), CreatedAtUtc, creatorId);
        var changedAtUtc = CreatedAtUtc.AddMinutes(1);
        var actorId = Guid.NewGuid();

        balance.ApplyAdjustment(5m, changedAtUtc, actorId);

        Assert.Equal(5m, balance.Quantity);
        Assert.Equal(1, balance.Version);
        Assert.Equal(changedAtUtc, balance.UpdatedAtUtc);
        Assert.Equal(actorId, balance.UpdatedByUserId);
    }

    [Fact]
    public void Apply_adjustment_rejects_a_negative_result()
    {
        var balance = InventoryBalance.Create(Guid.NewGuid(), Guid.NewGuid(), CreatedAtUtc);

        Assert.Throws<InvalidOperationException>(() => balance.ApplyAdjustment(-1m, CreatedAtUtc.AddMinutes(1)));
        Assert.Equal(0m, balance.Quantity);
        Assert.Equal(0, balance.Version);
    }

    [Fact]
    public void Create_movement_records_the_signed_delta_and_resulting_balance()
    {
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var movement = InventoryMovement.CreateManualAdjustment(
            productId,
            warehouseId,
            -2m,
            3m,
            CreatedAtUtc,
            Guid.NewGuid());

        Assert.Equal(productId, movement.ProductId);
        Assert.Equal(warehouseId, movement.WarehouseId);
        Assert.Equal(InventoryMovementType.ManualDecrease, movement.Type);
        Assert.Equal(-2m, movement.QuantityDelta);
        Assert.Equal(3m, movement.BalanceAfter);
    }
}
