using Warehouse.Domain.Common;

using Warehouse.Domain.Products;
namespace Warehouse.Domain.Inventory;

public sealed class InventoryMovement : PersistentEntity
{
    private InventoryMovement(
        Guid id,
        Guid productId,
        Guid warehouseId,
        InventoryMovementType type,
        string unitOfMeasure,
        decimal quantityDeltaInUnit,
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
        UnitOfMeasure = unitOfMeasure;
        QuantityDeltaInUnit = quantityDeltaInUnit;
        QuantityDelta = quantityDelta;
        BalanceAfter = balanceAfter;
    }

    public Guid ProductId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public InventoryMovementType Type { get; private set; }

    public decimal QuantityDelta { get; private set; }
    public string UnitOfMeasure { get; private set; } = null!;

    public decimal QuantityDeltaInUnit { get; private set; }


    public decimal BalanceAfter { get; private set; }

    public static InventoryMovement CreateManualAdjustment(
        Guid productId,
        Guid warehouseId,
        string? unitOfMeasure,
        decimal quantityDeltaInUnit,
        decimal quantityDelta,
        decimal balanceAfter,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        if (quantityDeltaInUnit == 0m || quantityDelta == 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityDelta), "Quantity must not be zero.");
        }

        if ((quantityDeltaInUnit < 0m) != (quantityDelta < 0m))
        {
            throw new ArgumentException("Quantity deltas must have the same direction.", nameof(quantityDeltaInUnit));
        }

        var normalizedUnitOfMeasure = ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure);

        return new InventoryMovement(
            Guid.NewGuid(),
            productId,
            warehouseId,
            quantityDelta > 0m ? InventoryMovementType.ManualIncrease : InventoryMovementType.ManualDecrease,
            normalizedUnitOfMeasure,
            quantityDeltaInUnit,
            quantityDelta,
            balanceAfter,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }
}
