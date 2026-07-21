using FluentValidation;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

public sealed class ProductListQueryValidator : PagedRequestValidator<ProductListQuery>
{
}

public sealed class ProductInputValidator : AbstractValidator<ProductInput>
{
    public ProductInputValidator()
    {
        RuleFor(input => input.Sku)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("SKU is required.")
            .WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(value => value is null || value.Trim().Length <= ProductRules.MaxSkuLength)
            .WithMessage($"SKU cannot exceed {ProductRules.MaxSkuLength} characters.")
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);

        RuleFor(input => input.Name)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Name is required.")
            .WithErrorCode(ApiErrorCodes.ValidationRequired)
            .Must(value => value is null || value.Trim().Length <= ProductRules.MaxNameLength)
            .WithMessage($"Name cannot exceed {ProductRules.MaxNameLength} characters.")
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);

        RuleFor(input => input.Description)
            .Must(value => value is null || value.Trim().Length <= ProductRules.MaxDescriptionLength)
            .WithMessage($"Description cannot exceed {ProductRules.MaxDescriptionLength} characters.")
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
}