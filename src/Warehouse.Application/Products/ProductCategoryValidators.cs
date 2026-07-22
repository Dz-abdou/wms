using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

public sealed class ProductCategoryListQueryValidator : PagedRequestValidator<ProductCategoryListQuery>
{
}

public sealed class ProductCategoryInputValidator : AbstractValidator<ProductCategoryInput>
{
    public ProductCategoryInputValidator()
    {
        RuleFor(input => input.Code)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Category code is required.")
            .WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(value => value is null || value.Trim().Length <= ProductCategoryRules.MaxCodeLength)
            .WithMessage($"Category code cannot exceed {ProductCategoryRules.MaxCodeLength} characters.")
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);

        RuleFor(input => input.Name)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Category name is required.")
            .WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(value => value is null || value.Trim().Length <= ProductCategoryRules.MaxNameLength)
            .WithMessage($"Category name cannot exceed {ProductCategoryRules.MaxNameLength} characters.")
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}

