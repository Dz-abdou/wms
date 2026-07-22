using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Products;

public sealed record ProductCategoryListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize) : IPagedRequest;

public sealed record ProductCategoryInput(string? Code, string? Name, Guid? ParentCategoryId);

public sealed record ProductCategoryResponse(
    Guid Id,
    string Code,
    string Name,
    Guid? ParentCategoryId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
