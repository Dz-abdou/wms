namespace Warehouse.Application.Warehouses;

public sealed record WarehouseListQuery(
    int Page = WarehouseConstants.DefaultPage,
    int PageSize = WarehouseConstants.DefaultPageSize);

public sealed record WarehouseInput(string? Code, string? Name, string? Description);

public sealed record SetWarehouseStatusRequest(bool IsActive);

public sealed record WarehouseResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);