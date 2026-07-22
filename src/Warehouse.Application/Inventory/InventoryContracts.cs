using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Inventory;

namespace Warehouse.Application.Inventory;

public enum InventoryAdjustmentDirection
{
    Increase,
    Decrease
}

public sealed record InventoryAdjustmentInput(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    InventoryAdjustmentDirection Direction);

public sealed record InventoryBalanceResponse(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    DateTime UpdatedAtUtc);

public sealed record InventoryMovementResponse(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    InventoryMovementType Type,
    decimal QuantityDelta,
    decimal BalanceAfter,
    DateTime CreatedAtUtc);

public sealed record InventoryMovementListQuery(
    Guid? ProductId,
    Guid? WarehouseId,
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize) : IPagedRequest;
