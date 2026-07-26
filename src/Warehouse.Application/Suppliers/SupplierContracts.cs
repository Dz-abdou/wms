using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Suppliers;

public sealed record SupplierListQuery(int Page = PaginationConstants.DefaultPage, int PageSize = PaginationConstants.DefaultPageSize, string? Search = null, bool? IsActive = null) : IPagedRequest;
public sealed record SupplierInput(string? Code, string? Name, string? Email, string? PhoneNumber, string? Address);
public sealed record SetSupplierStatusRequest(bool IsActive);
public sealed record SupplierResponse(Guid Id, string Code, string Name, string? Email, string? PhoneNumber, string? Address, bool IsActive, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
