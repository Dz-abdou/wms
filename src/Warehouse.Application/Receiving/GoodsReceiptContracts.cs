using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Receiving;

public sealed record GoodsReceiptListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    Guid? PurchaseOrderId = null,
    string? PurchaseOrderNumber = null,
    Guid? WarehouseId = null) : IPagedRequest;

public sealed record GoodsReceiptLineInput(Guid PurchaseOrderLineId, decimal AcceptedQuantity);

public sealed record GoodsReceiptInput(
    Guid PurchaseOrderId,
    int PurchaseOrderVersion,
    DateTime ReceivedAtUtc,
    string? SupplierDeliveryNote,
    string? Notes,
    IReadOnlyCollection<GoodsReceiptLineInput> Lines);

public sealed record GoodsReceiptResponse(
    Guid Id,
    string Number,
    Guid PurchaseOrderId,
    Guid WarehouseId,
    DateTime ReceivedAtUtc,
    int PurchaseOrderVersion);

public sealed record GoodsReceiptListItemResponse(
    Guid Id,
    string Number,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    DateTime ReceivedAtUtc,
    int LineCount);

public sealed record GoodsReceiptDetailResponse(
    Guid Id,
    string Number,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    DateTime ReceivedAtUtc,
    string? SupplierDeliveryNote,
    string? Notes,
    Guid ReceiverUserId,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<GoodsReceiptLineResponse> Lines);

public sealed record GoodsReceiptLineResponse(
    Guid Id,
    Guid PurchaseOrderLineId,
    int PurchaseOrderLineNumber,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string UnitOfMeasure,
    decimal AcceptedQuantity,
    decimal AcceptedQuantityInBaseUnit,
    decimal ConversionFactorToBaseUnit,
    Guid InventoryMovementId);

public sealed record GoodsReceiptCandidateResponse(
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string? CurrencyCode,
    int Version,
    IReadOnlyCollection<GoodsReceiptCandidateLineResponse> Lines);

public sealed record GoodsReceiptCandidateLineResponse(
    Guid PurchaseOrderLineId,
    int LineNumber,
    string ProductSku,
    string ProductName,
    string UnitOfMeasure,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal OutstandingQuantity,
    decimal ConversionFactorToBaseUnit);
