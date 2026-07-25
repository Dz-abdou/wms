namespace Warehouse.Application.Inventory;

public sealed class InventoryProductNotFoundException(Guid productId)
    : Exception($"Product '{productId}' was not found.");

public sealed class InventoryWarehouseNotFoundException(Guid warehouseId)
    : Exception($"Warehouse '{warehouseId}' was not found.");

public sealed class InventoryInvalidUnitOfMeasureException(Guid productId, string? unitOfMeasure)
    : Exception($"Unit of measure '{unitOfMeasure}' is not valid for product '{productId}'.");

public sealed class InsufficientInventoryException(Guid productId, Guid warehouseId)
    : Exception($"Insufficient inventory for product '{productId}' in warehouse '{warehouseId}'.");

public sealed class InventoryConcurrencyException(Exception innerException)
    : Exception("The inventory balance was changed by another operation.", innerException);

public sealed class InventoryAdjustmentNotFoundException(Guid adjustmentId)
    : Exception($"Inventory adjustment '{adjustmentId}' was not found.");
