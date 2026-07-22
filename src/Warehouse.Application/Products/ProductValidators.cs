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

        RuleFor(input => input.BaseUnitOfMeasure)
            .Must(IsSupportedBaseUnit)
            .WithMessage("A supported base unit of measure is required.")
            .WithErrorCode(ApiErrorCodes.ValidationInvalid);

        RuleForEach(input => input.UnitConversions!)
            .ChildRules(conversion =>
            {
                conversion.RuleFor(item => item.UnitOfMeasure)
                    .Must(IsValidUnit)
                    .WithMessage("A unit of measure is required.")
                    .WithErrorCode(ApiErrorCodes.ValidationInvalid);
                conversion.RuleFor(item => item.QuantityInBaseUnit)
                    .GreaterThan(0m)
                    .WithMessage("The conversion quantity must be greater than zero.")
                    .WithErrorCode(ApiErrorCodes.ValidationInvalid);
            });

        RuleFor(input => input)
            .Custom((input, context) =>
            {
                var conversions = input.UnitConversions ?? [];
                var normalizedUnits = conversions
                    .Select(conversion => conversion.UnitOfMeasure?.Trim().ToUpperInvariant())
                    .Where(unit => !string.IsNullOrWhiteSpace(unit))
                    .ToList();

                if (normalizedUnits.Distinct().Count() != normalizedUnits.Count)
                {
                    context.AddFailure(nameof(ProductInput.UnitConversions), "Each conversion unit of measure must be unique.");
                }

                if (input.BaseUnitOfMeasure is { } baseUnit &&
                    normalizedUnits.Contains(baseUnit.Trim().ToUpperInvariant()))
                {
                    context.AddFailure(nameof(ProductInput.UnitConversions), "The base unit of measure must not be repeated as a conversion.");
                }

                try
                {
                    var measurements = input.Measurements;
                    ProductMeasurements.Create(
                        measurements?.NetWeight,
                        measurements?.GrossWeight,
                        measurements?.WeightUnitOfMeasure,
                        measurements?.Length,
                        measurements?.Width,
                        measurements?.Height,
                        measurements?.DimensionUnitOfMeasure);
                }
                catch (ArgumentException exception)
                {
                    context.AddFailure(nameof(ProductInput.Measurements), exception.Message);
                }
            });
    }

    private static bool IsSupportedBaseUnit(string? value)
    {
        try
        {
            ProductUnitOfMeasure.NormalizeBaseUnitOfMeasure(value);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool IsValidUnit(string? value)
    {
        try
        {
            ProductUnitOfMeasure.NormalizeUnitOfMeasure(value);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
