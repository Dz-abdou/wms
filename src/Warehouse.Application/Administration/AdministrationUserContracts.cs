using Warehouse.Application.Common.Models;
using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Administration;

public sealed record AdministrationUserListQuery(
    int Page = PaginationConstants.DefaultPage,
    int PageSize = PaginationConstants.DefaultPageSize,
    string? Email = null,
    string? Role = null) : IPagedRequest;

public sealed record AdministrationUserResponse(
    Guid Id,
    string Email,
    IReadOnlyCollection<string> Roles);

public interface IAdministrationUserQueryService
{
    Task<PagedResult<AdministrationUserResponse>> GetListAsync(
        AdministrationUserListQuery query,
        CancellationToken cancellationToken);

    Task<AdministrationUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
