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

public sealed record InventoryAdjustmentListItemResponse(
    Guid Id,
    InventoryAdjustmentReason Reason,
    string? Reference,
    DateTime CreatedAtUtc,
    int LineCount);

public sealed record InventoryAdjustmentDetailResponse(
    Guid Id,
    InventoryAdjustmentReason Reason,
    string? Reference,
    string? Note,
    DateTime CreatedAtUtc,
    IReadOnlyList<InventoryAdjustmentLineResponse> Lines);

public sealed record InventoryAdjustmentLineResponse(
    Guid MovementId,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string Type,
    string UnitOfMeasure,
    decimal QuantityDeltaInUnit,
    decimal QuantityDelta,
    decimal BalanceAfter,
    DateTime CreatedAtUtc);

public sealed record InventoryMovementResponse(
    Guid Id,
    Guid? InventoryAdjustmentId,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string? AdjustmentReference,
    string Type,
    string UnitOfMeasure,
    decimal QuantityDeltaInUnit,
    decimal QuantityDelta,
    decimal BalanceAfter,
    DateTime CreatedAtUtc);

public sealed record InventoryMovementListQuery(
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    InventoryMovementType? Type = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    string? Reference = null,
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize) : IPagedRequest;

public sealed record InventoryAdjustmentListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize) : IPagedRequest;
