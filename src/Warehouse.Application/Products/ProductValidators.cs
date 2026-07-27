using FluentValidation;
using FluentValidation.Results;
using Warehouse.Application.Common.Errors;
using Warehouse.Application.Common.Pagination;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

public sealed class ProductListQueryValidator : PagedRequestValidator<ProductListQuery>
{
    public ProductListQueryValidator()
    {
        RuleFor(query => query.Search).MaximumLength(200)
            .WithErrorCode(ApiErrorCodes.ValidationMaxLength);
    }
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
                    AddFailure(context, nameof(ProductInput.UnitConversions), "Each conversion unit of measure must be unique.", ApiErrorCodes.ValidationInvalid);
                }

                if (input.BaseUnitOfMeasure is { } baseUnit &&
                    normalizedUnits.Contains(baseUnit.Trim().ToUpperInvariant()))
                {
                    AddFailure(context, nameof(ProductInput.UnitConversions), "The base unit of measure must not be repeated as a conversion.", ApiErrorCodes.ValidationInvalid);
                }

                ValidateMeasurements(input.Measurements, context);
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

    private static void ValidateMeasurements(ProductMeasurementsInput? measurements, ValidationContext<ProductInput> context)
    {
        if (measurements is null)
        {
            return;
        }

        var hasWeight = measurements.NetWeight is not null || measurements.GrossWeight is not null || !string.IsNullOrWhiteSpace(measurements.WeightUnitOfMeasure);
        if (hasWeight)
        {
            if (measurements.NetWeight is null && measurements.GrossWeight is null)
            {
                AddFailure(context, "Measurements.NetWeight", "Enter a net or gross weight when selecting a weight unit.", ApiErrorCodes.ProductMeasurementWeightRequired);
            }
            if (string.IsNullOrWhiteSpace(measurements.WeightUnitOfMeasure))
            {
                AddFailure(context, "Measurements.WeightUnitOfMeasure", "Select a weight unit when entering a weight.", ApiErrorCodes.ProductMeasurementWeightUnitRequired);
            }
            else if (!IsWeightUnit(measurements.WeightUnitOfMeasure))
            {
                AddFailure(context, "Measurements.WeightUnitOfMeasure", "Weight unit must be KG, G, or LB.", ApiErrorCodes.ProductMeasurementWeightUnitInvalid);
            }
            if (measurements.NetWeight is <= 0m)
            {
                AddFailure(context, "Measurements.NetWeight", "Net weight must be greater than zero.", ApiErrorCodes.ProductMeasurementWeightInvalid);
            }
            if (measurements.GrossWeight is <= 0m)
            {
                AddFailure(context, "Measurements.GrossWeight", "Gross weight must be greater than zero.", ApiErrorCodes.ProductMeasurementWeightInvalid);
            }
            if (measurements.NetWeight is { } netWeight && measurements.GrossWeight is { } grossWeight && grossWeight < netWeight)
            {
                AddFailure(context, "Measurements.GrossWeight", "Gross weight cannot be less than net weight.", ApiErrorCodes.ProductMeasurementGrossWeightInvalid);
            }
        }

        var hasDimensions = measurements.Length is not null || measurements.Width is not null || measurements.Height is not null || !string.IsNullOrWhiteSpace(measurements.DimensionUnitOfMeasure);
        if (!hasDimensions)
        {
            return;
        }

        if (measurements.Length is null)
        {
            AddFailure(context, "Measurements.Length", "Enter length when adding dimensions.", ApiErrorCodes.ProductMeasurementDimensionRequired);
        }
        if (measurements.Width is null)
        {
            AddFailure(context, "Measurements.Width", "Enter width when adding dimensions.", ApiErrorCodes.ProductMeasurementDimensionRequired);
        }
        if (measurements.Height is null)
        {
            AddFailure(context, "Measurements.Height", "Enter height when adding dimensions.", ApiErrorCodes.ProductMeasurementDimensionRequired);
        }
        if (string.IsNullOrWhiteSpace(measurements.DimensionUnitOfMeasure))
        {
            AddFailure(context, "Measurements.DimensionUnitOfMeasure", "Select a dimension unit when entering dimensions.", ApiErrorCodes.ProductMeasurementDimensionUnitRequired);
        }
        else if (!IsDimensionUnit(measurements.DimensionUnitOfMeasure))
        {
            AddFailure(context, "Measurements.DimensionUnitOfMeasure", "Dimension unit must be M, CM, or MM.", ApiErrorCodes.ProductMeasurementDimensionUnitInvalid);
        }
        if (measurements.Length is <= 0m)
        {
            AddFailure(context, "Measurements.Length", "Length must be greater than zero.", ApiErrorCodes.ProductMeasurementDimensionInvalid);
        }
        if (measurements.Width is <= 0m)
        {
            AddFailure(context, "Measurements.Width", "Width must be greater than zero.", ApiErrorCodes.ProductMeasurementDimensionInvalid);
        }
        if (measurements.Height is <= 0m)
        {
            AddFailure(context, "Measurements.Height", "Height must be greater than zero.", ApiErrorCodes.ProductMeasurementDimensionInvalid);
        }
    }

    private static bool IsWeightUnit(string value) => value.Trim().ToUpperInvariant() is "KG" or "G" or "LB";

    private static bool IsDimensionUnit(string value) => value.Trim().ToUpperInvariant() is "M" or "CM" or "MM";

    private static void AddFailure(ValidationContext<ProductInput> context, string propertyName, string message, string errorCode) =>
        context.AddFailure(new ValidationFailure(propertyName, message) { ErrorCode = errorCode });
}
