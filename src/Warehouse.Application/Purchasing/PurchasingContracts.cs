using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Purchasing;

namespace Warehouse.Application.Purchasing;

public sealed record SupplierProductListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    Guid? SupplierId = null,
    Guid? ProductId = null,
    bool? IsActive = null,
    string? CurrencyCode = null) : IPagedRequest;

public sealed record SupplierProductInput(
    Guid SupplierId,
    Guid ProductId,
    string? SupplierSku,
    string? PurchaseUnitOfMeasure,
    decimal MinimumOrderQuantity,
    decimal UnitPrice,
    string? CurrencyCode);

public sealed record UpdateSupplierProductInput(
    string? SupplierSku,
    string? PurchaseUnitOfMeasure,
    decimal MinimumOrderQuantity,
    decimal UnitPrice,
    string? CurrencyCode);

public sealed record SetSupplierProductStatusRequest(bool IsActive);

public sealed record SupplierProductResponse(
    Guid Id,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string? SupplierSku,
    string PurchaseUnitOfMeasure,
    decimal MinimumOrderQuantity,
    decimal UnitPrice,
    string CurrencyCode,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record PurchaseOrderListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    PurchaseOrderStatus? Status = null,
    Guid? SupplierId = null,
    Guid? WarehouseId = null,
    DateOnly? FromOrderDate = null,
    DateOnly? ToOrderDate = null) : IPagedRequest;

public sealed record PurchaseOrderLineInput(Guid SupplierProductId, decimal Quantity);

public sealed record PurchaseOrderVersionInput(int Version);

public sealed record PurchaseOrderCancelInput(int Version, string? Reason);

public sealed record PurchaseOrderInput(
    Guid SupplierId,
    Guid DestinationWarehouseId,
    string? CurrencyCode,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string? SupplierReference,
    string? Notes,
    int? Version,
    IReadOnlyCollection<PurchaseOrderLineInput>? Lines);

public sealed record PurchaseOrderLineResponse(
    Guid Id,
    int LineNumber,
    Guid SupplierProductId,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string? SupplierSku,
    string PurchaseUnitOfMeasure,
    decimal Quantity,
    decimal QuantityInBaseUnit,
    decimal ConversionFactorToBaseUnit,
    decimal UnitPrice,
    string CurrencyCode,
    decimal LineAmount);

public sealed record PurchaseOrderStatusHistoryResponse(
    Guid Id,
    PurchaseOrderStatus? PreviousStatus,
    PurchaseOrderStatus Status,
    DateTime ChangedAtUtc,
    Guid ActorUserId,
    string? Reason);

public sealed record PurchaseOrderResponse(
    Guid Id,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    string? Number,
    Guid? DestinationWarehouseId,
    string? DestinationWarehouseCode,
    string? DestinationWarehouseName,
    string? CurrencyCode,
    DateOnly? OrderDate,
    DateOnly? ExpectedDeliveryDate,
    Guid? BuyerUserId,
    string? SupplierReference,
    string? Notes,
    PurchaseOrderStatus Status,
    IReadOnlyCollection<PurchaseOrderLineResponse> Lines,
    decimal TotalAmount,
    int Version,
    DateTime? SubmittedAtUtc,
    IReadOnlyCollection<PurchaseOrderStatusHistoryResponse> StatusHistory,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
