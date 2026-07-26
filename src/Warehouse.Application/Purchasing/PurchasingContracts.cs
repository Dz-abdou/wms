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
    Guid? SupplierId = null) : IPagedRequest;

public sealed record PurchaseOrderLineInput(Guid SupplierProductId, decimal Quantity);

public sealed record PurchaseOrderInput(Guid SupplierId, IReadOnlyCollection<PurchaseOrderLineInput>? Lines);

public sealed record PurchaseOrderLineResponse(
    Guid Id,
    Guid SupplierProductId,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string? SupplierSku,
    string PurchaseUnitOfMeasure,
    decimal Quantity,
    decimal UnitPrice,
    string CurrencyCode);

public sealed record PurchaseOrderResponse(
    Guid Id,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    PurchaseOrderStatus Status,
    IReadOnlyCollection<PurchaseOrderLineResponse> Lines,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
