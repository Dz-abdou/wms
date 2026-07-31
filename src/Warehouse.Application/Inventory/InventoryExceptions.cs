namespace Warehouse.Application.Inventory;

public sealed class InventoryProductNotFoundException(Guid productId)
    : Exception($"Product '{productId}' was not found.");

public sealed class InventoryWarehouseNotFoundException(Guid warehouseId)
    : Exception($"Warehouse '{warehouseId}' was not found.");

public sealed class InventoryInvalidUnitOfMeasureException(Guid productId, string? unitOfMeasure)
    : Exception($"Unit of measure '{unitOfMeasure}' is not valid for product '{productId}'.");

public sealed class InsufficientInventoryException(
    int lineIndex,
    Guid productId,
    Guid warehouseId,
    decimal availableQuantity,
    string baseUnitOfMeasure,
    string warehouseCode,
    string warehouseName)
    : Exception($"Insufficient inventory for product '{productId}' in warehouse '{warehouseId}'.")
{
    public string PropertyName => $"Lines[{lineIndex}].Quantity";

    public decimal AvailableQuantity { get; } = availableQuantity;

    public string BaseUnitOfMeasure { get; } = baseUnitOfMeasure;

    public string Warehouse { get; } = $"{warehouseCode} — {warehouseName}";
}

public sealed class InventoryConcurrencyException(Exception innerException)
    : Exception("The inventory balance was changed by another operation.", innerException);

public sealed class InventoryAdjustmentNotFoundException(Guid adjustmentId)
    : Exception($"Inventory adjustment '{adjustmentId}' was not found.");

public sealed class CycleCountNotFoundException(Guid cycleCountId)
    : Exception($"Cycle count '{cycleCountId}' was not found.");

public sealed class InventoryTransferNotFoundException(Guid inventoryTransferId)
    : Exception($"Inventory transfer '{inventoryTransferId}' was not found.");

public sealed class InventoryTransferStaleBalanceException(
    int lineIndex,
    decimal currentQuantityInBase,
    string baseUnitOfMeasure,
    string warehouse)
    : Exception("Source inventory changed after this transfer line was loaded.")
{
    public string PropertyName => $"Lines[{lineIndex}].Quantity";

    public decimal CurrentQuantityInBase { get; } = currentQuantityInBase;

    public string BaseUnitOfMeasure { get; } = baseUnitOfMeasure;

    public string Warehouse { get; } = warehouse;
}

public sealed class CycleCountStaleBalanceException(
    int lineIndex,
    decimal currentQuantityInBase,
    string baseUnitOfMeasure)
    : Exception("Inventory changed after this count line was loaded.")
{
    public string PropertyName => $"Lines[{lineIndex}].SystemQuantityInBase";

    public decimal CurrentQuantityInBase { get; } = currentQuantityInBase;

    public string BaseUnitOfMeasure { get; } = baseUnitOfMeasure;
}
