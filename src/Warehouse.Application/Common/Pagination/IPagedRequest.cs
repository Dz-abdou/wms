namespace Warehouse.Application.Common.Pagination;

public interface IPagedRequest
{
    int Page { get; }

    int PageSize { get; }
}
