namespace Warehouse.Application.Products;

public sealed record ProductListQuery(
    int Page = ProductConstants.DefaultPage,
    int PageSize = ProductConstants.DefaultPageSize,
    string? Search = null);

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