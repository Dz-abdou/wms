using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Products;

public sealed record ProductListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    string? Search = null) : IPagedRequest;

public sealed record ProductUnitConversionInput(string? UnitOfMeasure, decimal QuantityInBaseUnit);

public sealed record ProductMeasurementsInput(
    decimal? NetWeight,
    decimal? GrossWeight,
    string? WeightUnitOfMeasure,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    string? DimensionUnitOfMeasure);

public sealed record ProductInput(
    string? Sku,
    string? Name,
    string? Description,
    string? BaseUnitOfMeasure = "EA",
    IReadOnlyCollection<ProductUnitConversionInput>? UnitConversions = null,
    ProductMeasurementsInput? Measurements = null,
    Guid? CategoryId = null);

public sealed record SetProductStatusRequest(bool IsActive);

public sealed record ProductUnitConversionResponse(string UnitOfMeasure, decimal QuantityInBaseUnit);

public sealed record ProductMeasurementsResponse(
    decimal? NetWeight,
    decimal? GrossWeight,
    string? WeightUnitOfMeasure,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    string? DimensionUnitOfMeasure,
    decimal? VolumeCubicMetres);

public sealed record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string BaseUnitOfMeasure,
    IReadOnlyCollection<ProductUnitConversionResponse> UnitConversions,
    ProductMeasurementsResponse? Measurements,
    bool IsActive,
    Guid? CategoryId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
