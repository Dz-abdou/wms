using Warehouse.Domain.Inventory;

namespace Warehouse.UnitTests.Inventory;

public sealed class CycleCountTests
{
    private static readonly DateTime CountedAtUtc = new(2026, 7, 30, 14, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_line_preserves_snapshot_and_calculates_variance()
    {
        var cycleCountId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var line = CycleCountLine.Create(
            cycleCountId,
            1,
            productId,
            10m,
            3,
            "EA",
            7m,
            7m,
            CountedAtUtc,
            Guid.NewGuid());

        Assert.Equal(cycleCountId, line.CycleCountId);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal(10m, line.SystemQuantityInBase);
        Assert.Equal(3, line.SystemBalanceVersion);
        Assert.Equal(-3m, line.VarianceQuantityInBase);
        Assert.Null(line.InventoryMovementId);
    }

    [Fact]
    public void Create_cycle_count_movement_sets_the_signed_cycle_count_type()
    {
        var cycleCountId = Guid.NewGuid();
        var movement = InventoryMovement.CreateCycleCount(
            cycleCountId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "EA",
            -3m,
            -3m,
            7m,
            CountedAtUtc,
            Guid.NewGuid());

        Assert.Equal(cycleCountId, movement.CycleCountId);
        Assert.Equal(InventoryMovementType.CycleCountDecrease, movement.Type);
        Assert.Equal(-3m, movement.QuantityDeltaInUnit);
        Assert.Equal(-3m, movement.QuantityDelta);
        Assert.Equal(7m, movement.BalanceAfter);
    }
}
