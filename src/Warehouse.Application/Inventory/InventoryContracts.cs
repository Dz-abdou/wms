using System.Text.Json.Serialization;
using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Inventory;

[JsonConverter(typeof(JsonStringEnumConverter))]
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
    string Type,
    decimal QuantityDelta,
    decimal BalanceAfter,
    DateTime CreatedAtUtc);

public sealed record InventoryMovementListQuery(
    Guid? ProductId,
    Guid? WarehouseId,
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize) : IPagedRequest;
