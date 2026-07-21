using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Domain.Warehouses;

namespace Warehouse.Application.Warehouses;

public sealed class WarehouseListQueryValidator : AbstractValidator<WarehouseListQuery>
{
    public WarehouseListQueryValidator()
    {
        RuleFor(query => query.Page).InclusiveBetween(WarehouseConstants.DefaultPage, WarehouseConstants.MaxPageNumber);
        RuleFor(query => query.PageSize).InclusiveBetween(1, WarehouseConstants.MaxPageSize);
    }
}

public sealed class WarehouseInputValidator : AbstractValidator<WarehouseInput>
{
    public WarehouseInputValidator()
    {
        RuleFor(input => input.Code)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Warehouse code is required.")
            .WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(value => value is null || value.Trim().Length <= WarehouseRules.MaxCodeLength)
            .WithMessage($"Warehouse code cannot exceed {WarehouseRules.MaxCodeLength} characters.")
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);

        RuleFor(input => input.Name)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Warehouse name is required.")
            .WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(value => value is null || value.Trim().Length <= WarehouseRules.MaxNameLength)
            .WithMessage($"Warehouse name cannot exceed {WarehouseRules.MaxNameLength} characters.")
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);

        RuleFor(input => input.Description)
            .Must(value => value is null || value.Trim().Length <= WarehouseRules.MaxDescriptionLength)
            .WithMessage($"Warehouse description cannot exceed {WarehouseRules.MaxDescriptionLength} characters.")
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}