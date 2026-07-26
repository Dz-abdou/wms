using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;

namespace Warehouse.Application.Administration;

public sealed class AdministrationUserListQueryValidator
    : PagedRequestValidator<AdministrationUserListQuery>
{
    public AdministrationUserListQueryValidator()
    {
        RuleFor(query => query.Email).MaximumLength(320)
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
        RuleFor(query => query.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) ||
                role is "admin" or "manager" or "operator")
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);
    }
}
