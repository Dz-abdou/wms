namespace Warehouse.Application.Warehouses;

public sealed class WarehouseNotFoundException(Guid warehouseId)
    : Exception($"Warehouse '{warehouseId}' was not found.");

public sealed class WarehouseCodeConflictException(string code, Exception? innerException = null)
    : Exception($"A warehouse with code '{code}' already exists.", innerException);