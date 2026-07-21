using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Products;

public sealed record ProductListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    string? Search = null) : IPagedRequest;

public sealed record ProductInput(string? Sku, string? Name, string? Description);

public sealed record SetProductStatusRequest(bool IsActive);

public sealed record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);