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

public sealed record InventoryTransferLineInput(
    Guid ProductId,
    decimal Quantity,
    string? UnitOfMeasure,
    decimal SourceQuantityInBase,
    int SourceBalanceVersion);

public sealed record InventoryTransferInput(
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string? Reference,
    string? Note,
    IReadOnlyList<InventoryTransferLineInput> Lines);

public sealed record InventoryTransferCandidateQuery(
    Guid SourceWarehouseId,
    Guid ProductId);

public sealed record InventoryTransferCandidateResponse(
    Guid ProductId,
    string BaseUnitOfMeasure,
    decimal AvailableQuantityInBase,
    int SourceBalanceVersion);

public sealed record InventoryBalanceResponse(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    DateTime UpdatedAtUtc,
    string BaseUnitOfMeasure);

public sealed record InventoryOverviewItemResponse(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    bool ProductIsActive,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    decimal Quantity,
    string BaseUnitOfMeasure,
    DateTime UpdatedAtUtc);

public sealed record InventoryAdjustmentResponse(
    Guid Id,
    string Number,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    InventoryAdjustmentReason Reason,
    string? Reference,
    string? Note,
    DateTime CreatedAtUtc,
    IReadOnlyList<InventoryBalanceResponse> Lines);

public sealed record InventoryAdjustmentListItemResponse(
    Guid Id,
    string Number,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    InventoryAdjustmentReason Reason,
    string? Reference,
    DateTime CreatedAtUtc,
    int LineCount);

public sealed record InventoryAdjustmentDetailResponse(
    Guid Id,
    string Number,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
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
    DateTime CreatedAtUtc)
{
    public int LineNumber { get; init; }
}

public sealed record InventoryMovementResponse(
    Guid Id,
    Guid? InventoryAdjustmentId,
    Guid? GoodsReceiptId,
    Guid? CycleCountId,
    Guid? InventoryTransferId,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string? DocumentNumber,
    string? ExternalReference,
    string? AdjustmentReference,
    string? GoodsReceiptNumber,
    string? CycleCountReference,
    string? TransferReference,
    string Type,
    string UnitOfMeasure,
    decimal QuantityDeltaInUnit,
    decimal QuantityDelta,
    decimal BalanceAfter,
    DateTime CreatedAtUtc);

public sealed record InventoryTransferListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    Guid? SourceWarehouseId = null,
    Guid? DestinationWarehouseId = null,
    string? Reference = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IPagedRequest;

public sealed record InventoryTransferListItemResponse(
    Guid Id,
    string Number,
    Guid SourceWarehouseId,
    string SourceWarehouseCode,
    string SourceWarehouseName,
    Guid DestinationWarehouseId,
    string DestinationWarehouseCode,
    string DestinationWarehouseName,
    string? Reference,
    DateTime TransferredAtUtc,
    int LineCount);

public sealed record InventoryTransferDetailResponse(
    Guid Id,
    string Number,
    Guid SourceWarehouseId,
    string SourceWarehouseCode,
    string SourceWarehouseName,
    Guid DestinationWarehouseId,
    string DestinationWarehouseCode,
    string DestinationWarehouseName,
    string? Reference,
    string? Note,
    DateTime TransferredAtUtc,
    IReadOnlyList<InventoryTransferLineResponse> Lines);

public sealed record InventoryTransferLineResponse(
    Guid Id,
    int LineNumber,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string UnitOfMeasure,
    decimal QuantityInUnit,
    decimal QuantityInBaseUnit,
    Guid TransferOutMovementId,
    decimal SourceBalanceAfter,
    Guid TransferInMovementId,
    decimal DestinationBalanceAfter);

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
    int PageSize = PaginationConstants.DefaultPageSize,
    InventoryAdjustmentReason? Reason = null,
    string? Reference = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IPagedRequest;

public sealed record InventoryOverviewQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    string? Search = null,
    Guid? WarehouseId = null,
    Guid? CategoryId = null,
    bool? IsActive = null) : IPagedRequest;

public sealed record CycleCountLineInput(
    Guid ProductId,
    decimal SystemQuantityInBase,
    int SystemBalanceVersion,
    string? CountedUnitOfMeasure,
    decimal CountedQuantityInUnit);

public sealed record CycleCountInput(
    Guid WarehouseId,
    string? Reference,
    string? Note,
    IReadOnlyList<CycleCountLineInput> Lines);

public sealed record CycleCountCandidateResponse(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string BaseUnitOfMeasure,
    decimal SystemQuantityInBase,
    int SystemBalanceVersion);

public sealed record CycleCountCandidateQuery(Guid WarehouseId, Guid ProductId);

public sealed record CycleCountListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    Guid? WarehouseId = null,
    string? Reference = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IPagedRequest;

public sealed record CycleCountListItemResponse(
    Guid Id,
    string Number,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string? Reference,
    DateTime CountedAtUtc,
    int LineCount,
    int VarianceLineCount);

public sealed record CycleCountDetailResponse(
    Guid Id,
    string Number,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string? Reference,
    string? Note,
    DateTime CountedAtUtc,
    IReadOnlyList<CycleCountLineResponse> Lines);

public sealed record CycleCountLineResponse(
    Guid Id,
    int LineNumber,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal SystemQuantityInBase,
    int SystemBalanceVersion,
    string BaseUnitOfMeasure,
    string CountedUnitOfMeasure,
    decimal CountedQuantityInUnit,
    decimal CountedQuantityInBase,
    decimal VarianceQuantityInBase,
    Guid? InventoryMovementId);
