using System.Text.Json.Serialization;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Inventory;

namespace Warehouse.Application.Inventory;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InventoryAdjustmentDirection
{
    Increase,
    Decrease
}

public sealed record InventoryAdjustmentLineInput(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    InventoryAdjustmentDirection Direction,
    string? UnitOfMeasure);

public sealed record InventoryAdjustmentInput(
    InventoryAdjustmentReason Reason,
    string? Reference,
    string? Note,
    IReadOnlyList<InventoryAdjustmentLineInput> Lines);

public sealed record InventoryBalanceResponse(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    DateTime UpdatedAtUtc,
    string BaseUnitOfMeasure);

public sealed record InventoryAdjustmentResponse(
    Guid Id,
    InventoryAdjustmentReason Reason,
    string? Reference,
    string? Note,
    DateTime CreatedAtUtc,
    IReadOnlyList<InventoryBalanceResponse> Lines);

public sealed record InventoryMovementResponse(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    string Type,
    string UnitOfMeasure,
    decimal QuantityDeltaInUnit,
    decimal QuantityDelta,
    decimal BalanceAfter,
    DateTime CreatedAtUtc);

public sealed record InventoryMovementListQuery(
    Guid? ProductId,
    Guid? WarehouseId,
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize) : IPagedRequest;
