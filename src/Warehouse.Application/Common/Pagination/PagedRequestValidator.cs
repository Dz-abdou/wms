using FluentValidation;

namespace Warehouse.Application.Common.Pagination;

public abstract class PagedRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : IPagedRequest
{
    protected PagedRequestValidator()
    {
        RuleFor(request => request.Page)
            .InclusiveBetween(
                PaginationConstants.DefaultPage,
                PaginationConstants.MaxPageNumber);

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, PaginationConstants.MaxPageSize);
    }
}
