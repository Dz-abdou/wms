using Warehouse.Domain.Common;

using Warehouse.Domain.Products;
namespace Warehouse.Domain.Inventory;

public sealed class InventoryMovement : PersistentEntity
{
    private InventoryMovement(
        Guid id,
        Guid? inventoryAdjustmentId,
        Guid? goodsReceiptId,
        Guid? cycleCountId,
        Guid? inventoryTransferId,
        Guid productId,
        Guid warehouseId,
        InventoryMovementType type,
        string unitOfMeasure,
        decimal quantityDeltaInUnit,
        decimal quantityDelta,
        decimal balanceAfter,
        int? lineNumber,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        InventoryAdjustmentId = inventoryAdjustmentId;
        GoodsReceiptId = goodsReceiptId;
        CycleCountId = cycleCountId;
        InventoryTransferId = inventoryTransferId;
        ProductId = productId;
        WarehouseId = warehouseId;
        Type = type;
        UnitOfMeasure = unitOfMeasure;
        QuantityDeltaInUnit = quantityDeltaInUnit;
        QuantityDelta = quantityDelta;
        BalanceAfter = balanceAfter;
        LineNumber = lineNumber;
    }

    public Guid ProductId { get; private set; }

    public Guid? InventoryAdjustmentId { get; private set; }

    public Guid? GoodsReceiptId { get; private set; }

    public Guid? CycleCountId { get; private set; }

    public Guid? InventoryTransferId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public InventoryMovementType Type { get; private set; }

    public decimal QuantityDelta { get; private set; }
    public string UnitOfMeasure { get; private set; } = null!;

    public decimal QuantityDeltaInUnit { get; private set; }


    public decimal BalanceAfter { get; private set; }

    public int? LineNumber { get; private set; }

    public static InventoryMovement CreateManualAdjustment(
        Guid productId,
        Guid warehouseId,
        string? unitOfMeasure,
        decimal quantityDeltaInUnit,
        decimal quantityDelta,
        decimal balanceAfter,
        DateTime createdAtUtc,
        Guid? actorUserId = null,
        Guid? inventoryAdjustmentId = null,
        int? lineNumber = null)
    {
        if (quantityDeltaInUnit == 0m || quantityDelta == 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityDelta), "Quantity must not be zero.");
        }

        if ((quantityDeltaInUnit < 0m) != (quantityDelta < 0m))
        {
            throw new ArgumentException("Quantity deltas must have the same direction.", nameof(quantityDeltaInUnit));
        }

        if (lineNumber is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lineNumber));
        }

        var normalizedUnitOfMeasure = ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure);

        return new InventoryMovement(
            Guid.NewGuid(),
            inventoryAdjustmentId,
            null,
            null,
            null,
            productId,
            warehouseId,
            quantityDelta > 0m ? InventoryMovementType.ManualIncrease : InventoryMovementType.ManualDecrease,
            normalizedUnitOfMeasure,
            quantityDeltaInUnit,
            quantityDelta,
            balanceAfter,
            lineNumber,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public static InventoryMovement CreateGoodsReceipt(
        Guid goodsReceiptId,
        Guid productId,
        Guid warehouseId,
        string? unitOfMeasure,
        decimal quantityReceivedInUnit,
        decimal quantityReceivedInBaseUnit,
        decimal balanceAfter,
        DateTime receivedAtUtc,
        Guid? actorUserId = null)
    {
        if (goodsReceiptId == Guid.Empty || quantityReceivedInUnit <= 0m || quantityReceivedInBaseUnit <= 0m)
            throw new ArgumentOutOfRangeException(nameof(quantityReceivedInUnit));

        return new InventoryMovement(
            Guid.NewGuid(),
            null,
            goodsReceiptId,
            null,
            null,
            productId,
            warehouseId,
            InventoryMovementType.GoodsReceipt,
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure),
            quantityReceivedInUnit,
            quantityReceivedInBaseUnit,
            balanceAfter,
            null,
            receivedAtUtc,
            receivedAtUtc,
            actorUserId,
            actorUserId);
    }

    public static InventoryMovement CreateCycleCount(
        Guid cycleCountId,
        Guid productId,
        Guid warehouseId,
        string? unitOfMeasure,
        decimal quantityDeltaInUnit,
        decimal quantityDelta,
        decimal balanceAfter,
        DateTime countedAtUtc,
        Guid? actorUserId = null)
    {
        if (cycleCountId == Guid.Empty || quantityDeltaInUnit == 0m || quantityDelta == 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityDelta), "Cycle-count movement quantities must not be zero.");
        }

        if ((quantityDeltaInUnit < 0m) != (quantityDelta < 0m))
        {
            throw new ArgumentException("Quantity deltas must have the same direction.", nameof(quantityDeltaInUnit));
        }

        return new InventoryMovement(
            Guid.NewGuid(),
            null,
            null,
            cycleCountId,
            null,
            productId,
            warehouseId,
            quantityDelta > 0m
                ? InventoryMovementType.CycleCountIncrease
                : InventoryMovementType.CycleCountDecrease,
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure),
            quantityDeltaInUnit,
            quantityDelta,
            balanceAfter,
            null,
            countedAtUtc,
            countedAtUtc,
            actorUserId,
            actorUserId);
    }

    public static InventoryMovement CreateTransferOut(
        Guid inventoryTransferId,
        Guid productId,
        Guid warehouseId,
        string? unitOfMeasure,
        decimal quantityInUnit,
        decimal quantityInBaseUnit,
        decimal balanceAfter,
        DateTime transferredAtUtc,
        Guid? actorUserId = null) =>
        CreateTransferMovement(
            inventoryTransferId,
            productId,
            warehouseId,
            unitOfMeasure,
            -quantityInUnit,
            -quantityInBaseUnit,
            balanceAfter,
            transferredAtUtc,
            actorUserId,
            InventoryMovementType.TransferOut);

    public static InventoryMovement CreateTransferIn(
        Guid inventoryTransferId,
        Guid productId,
        Guid warehouseId,
        string? unitOfMeasure,
        decimal quantityInUnit,
        decimal quantityInBaseUnit,
        decimal balanceAfter,
        DateTime transferredAtUtc,
        Guid? actorUserId = null) =>
        CreateTransferMovement(
            inventoryTransferId,
            productId,
            warehouseId,
            unitOfMeasure,
            quantityInUnit,
            quantityInBaseUnit,
            balanceAfter,
            transferredAtUtc,
            actorUserId,
            InventoryMovementType.TransferIn);

    private static InventoryMovement CreateTransferMovement(
        Guid inventoryTransferId,
        Guid productId,
        Guid warehouseId,
        string? unitOfMeasure,
        decimal quantityDeltaInUnit,
        decimal quantityDelta,
        decimal balanceAfter,
        DateTime transferredAtUtc,
        Guid? actorUserId,
        InventoryMovementType type)
    {
        if (inventoryTransferId == Guid.Empty || quantityDeltaInUnit == 0m || quantityDelta == 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityDelta));
        }

        if ((quantityDeltaInUnit < 0m) != (quantityDelta < 0m))
        {
            throw new ArgumentException("Quantity deltas must have the same direction.", nameof(quantityDeltaInUnit));
        }

        return new InventoryMovement(
            Guid.NewGuid(),
            null,
            null,
            null,
            inventoryTransferId,
            productId,
            warehouseId,
            type,
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure),
            quantityDeltaInUnit,
            quantityDelta,
            balanceAfter,
            null,
            transferredAtUtc,
            transferredAtUtc,
            actorUserId,
            actorUserId);
    }
}
