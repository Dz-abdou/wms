using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Currencies;

public sealed record CurrencyListQuery(int Page = PaginationConstants.DefaultPage, int PageSize = PaginationConstants.DefaultPageSize, bool ActiveOnly = false) : IPagedRequest;
public sealed record CurrencyInput(string? Code, string? Name, string? Symbol, int DecimalPlaces);
public sealed record UpdateCurrencyInput(string? Name, string? Symbol, int DecimalPlaces);
public sealed record SetCurrencyStatusRequest(bool IsActive);
public sealed record CurrencyResponse(Guid Id, string Code, string Name, string? Symbol, int DecimalPlaces, bool IsActive, bool IsDefault, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
