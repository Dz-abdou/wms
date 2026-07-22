using Warehouse.Domain.Common;

namespace Warehouse.Domain.Inventory;

public sealed class InventoryMovement : PersistentEntity
{
    private InventoryMovement(
        Guid id,
        Guid productId,
        Guid warehouseId,
        InventoryMovementType type,
        decimal quantityDelta,
        decimal balanceAfter,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        Type = type;
        QuantityDelta = quantityDelta;
        BalanceAfter = balanceAfter;
    }

    public Guid ProductId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public InventoryMovementType Type { get; private set; }

    public decimal QuantityDelta { get; private set; }

    public decimal BalanceAfter { get; private set; }

    public static InventoryMovement CreateManualAdjustment(
        Guid productId,
        Guid warehouseId,
        decimal quantityDelta,
        decimal balanceAfter,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        if (quantityDelta == 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityDelta), "Quantity must not be zero.");
        }

        return new InventoryMovement(
            Guid.NewGuid(),
            productId,
            warehouseId,
            quantityDelta > 0m ? InventoryMovementType.ManualIncrease : InventoryMovementType.ManualDecrease,
            quantityDelta,
            balanceAfter,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }
}
