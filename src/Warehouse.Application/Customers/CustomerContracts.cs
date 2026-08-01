using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Customers;

public sealed record CustomerListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    string? Search = null,
    bool? IsActive = null) : IPagedRequest;

public sealed record CustomerInput(
    string? Code,
    string? LegalName,
    string? TradingName,
    string? DefaultCurrencyCode,
    string? DeliveryInstructions,
    string? ServiceNotes);

public sealed record CustomerContactInput(string? Name, string? Role, string? Email, string? PhoneNumber);

public sealed record CustomerAddressInput(
    string? Label,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? PostalCode,
    string? CountryCode,
    bool IsShippingAddress,
    bool IsBillingAddress,
    string? DeliveryInstructions);

public sealed record SetCustomerStatusRequest(bool IsActive);

public sealed record CustomerListItemResponse(Guid Id, string Code, string LegalName, string? TradingName, string? DefaultCurrencyCode, bool IsActive);

public sealed record CustomerResponse(
    Guid Id,
    string Code,
    string LegalName,
    string? TradingName,
    string? DefaultCurrencyCode,
    string? DeliveryInstructions,
    string? ServiceNotes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<CustomerContactResponse> Contacts,
    IReadOnlyList<CustomerAddressResponse> Addresses);

public sealed record CustomerContactResponse(Guid Id, string Name, string? Role, string? Email, string? PhoneNumber);

public sealed record CustomerAddressResponse(
    Guid Id,
    string Label,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? PostalCode,
    string CountryCode,
    bool IsShippingAddress,
    bool IsBillingAddress,
    string? DeliveryInstructions);
