using System.Text.Json.Serialization;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Sales;

namespace Warehouse.Application.Sales;

public sealed record SalesOrderLineInput(Guid ProductId, string? UnitOfMeasure, decimal Quantity);
public sealed record SalesOrderInput(Guid CustomerId, Guid ShippingAddressId, string? CurrencyCode, DateOnly OrderDate, DateOnly? RequestedShipDate, string? CustomerReference, string? DeliveryInstructions, int? Version, IReadOnlyList<SalesOrderLineInput>? Lines);
public sealed record SalesOrderVersionInput(int Version);
public sealed record SalesOrderCancelInput(int Version, string? Reason);
public sealed record SalesOrderListQuery(int Page = PaginationConstants.DefaultPage, int PageSize = PaginationConstants.DefaultPageSize, SalesOrderStatus? Status = null, Guid? CustomerId = null, DateOnly? FromOrderDate = null, DateOnly? ToOrderDate = null) : IPagedRequest;
public sealed record SalesOrderLineResponse(Guid Id, int LineNumber, Guid ProductId, string ProductSku, string ProductName, string UnitOfMeasure, decimal Quantity, decimal QuantityInBaseUnit, decimal ConversionFactorToBaseUnit);
public sealed record SalesOrderAddressResponse(string Label, string AddressLine1, string? AddressLine2, string City, string? PostalCode, string CountryCode, string? DeliveryInstructions);
public sealed record SalesOrderStatusHistoryResponse(Guid Id, [property: JsonConverter(typeof(JsonStringEnumConverter))] SalesOrderStatus? PreviousStatus, [property: JsonConverter(typeof(JsonStringEnumConverter))] SalesOrderStatus Status, DateTime ChangedAtUtc, Guid ActorUserId, string? Reason);
public sealed record SalesOrderResponse(Guid Id, string Number, Guid CustomerId, string CustomerCode, string CustomerName, Guid ShippingAddressId, SalesOrderAddressResponse ShippingAddress, string CurrencyCode, DateOnly OrderDate, DateOnly? RequestedShipDate, string? CustomerReference, string? DeliveryInstructions, Guid OwnerUserId, [property: JsonConverter(typeof(JsonStringEnumConverter))] SalesOrderStatus Status, IReadOnlyList<SalesOrderLineResponse> Lines, int Version, DateTime? SubmittedAtUtc, IReadOnlyList<SalesOrderStatusHistoryResponse> StatusHistory, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
