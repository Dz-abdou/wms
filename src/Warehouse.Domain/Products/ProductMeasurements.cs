namespace Warehouse.Domain.Products;

public sealed class ProductMeasurements
{
    private ProductMeasurements(
        decimal? netWeight,
        decimal? grossWeight,
        string? weightUnitOfMeasure,
        decimal? length,
        decimal? width,
        decimal? height,
        string? dimensionUnitOfMeasure)
    {
        NetWeight = netWeight;
        GrossWeight = grossWeight;
        WeightUnitOfMeasure = weightUnitOfMeasure;
        Length = length;
        Width = width;
        Height = height;
        DimensionUnitOfMeasure = dimensionUnitOfMeasure;
    }

    public decimal? NetWeight { get; private set; }

    public decimal? GrossWeight { get; private set; }

    public string? WeightUnitOfMeasure { get; private set; }

    public decimal? Length { get; private set; }

    public decimal? Width { get; private set; }

    public decimal? Height { get; private set; }

    public string? DimensionUnitOfMeasure { get; private set; }

    public decimal? VolumeCubicMetres => Length is { } length &&
        Width is { } width &&
        Height is { } height &&
        DimensionUnitOfMeasure is { } unit
            ? length * width * height * GetMetresPerUnit(unit) * GetMetresPerUnit(unit) * GetMetresPerUnit(unit)
            : null;

    public bool IsSameAs(ProductMeasurements? other) =>
        other is not null &&
        NetWeight == other.NetWeight &&
        GrossWeight == other.GrossWeight &&
        WeightUnitOfMeasure == other.WeightUnitOfMeasure &&
        Length == other.Length &&
        Width == other.Width &&
        Height == other.Height &&
        DimensionUnitOfMeasure == other.DimensionUnitOfMeasure;

    public static ProductMeasurements? Create(
        decimal? netWeight,
        decimal? grossWeight,
        string? weightUnitOfMeasure,
        decimal? length,
        decimal? width,
        decimal? height,
        string? dimensionUnitOfMeasure)
    {
        var hasWeight = netWeight is not null || grossWeight is not null || !string.IsNullOrWhiteSpace(weightUnitOfMeasure);
        var hasDimensions = length is not null || width is not null || height is not null || !string.IsNullOrWhiteSpace(dimensionUnitOfMeasure);
        if (!hasWeight && !hasDimensions)
        {
            return null;
        }

        var normalizedWeightUnit = ValidateWeight(netWeight, grossWeight, weightUnitOfMeasure);
        var normalizedDimensionUnit = ValidateDimensions(length, width, height, dimensionUnitOfMeasure);

        return new ProductMeasurements(
            netWeight,
            grossWeight,
            normalizedWeightUnit,
            length,
            width,
            height,
            normalizedDimensionUnit);
    }

    private static string? ValidateWeight(decimal? netWeight, decimal? grossWeight, string? unitOfMeasure)
    {
        if (netWeight is null && grossWeight is null && string.IsNullOrWhiteSpace(unitOfMeasure))
        {
            return null;
        }

        if (netWeight is null && grossWeight is null || string.IsNullOrWhiteSpace(unitOfMeasure))
        {
            throw new ArgumentException("A weight unit is required when a product weight is supplied.", nameof(unitOfMeasure));
        }

        if (netWeight is <= 0m || grossWeight is <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(netWeight), "Weights must be greater than zero.");
        }

        if (grossWeight is { } gross && netWeight is { } net && gross < net)
        {
            throw new ArgumentException("Gross weight cannot be less than net weight.", nameof(grossWeight));
        }

        var normalized = ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure);
        if (normalized is not ("KG" or "G" or "LB"))
        {
            throw new ArgumentException("Weight unit must be KG, G, or LB.", nameof(unitOfMeasure));
        }

        return normalized;
    }

    private static string? ValidateDimensions(
        decimal? length,
        decimal? width,
        decimal? height,
        string? unitOfMeasure)
    {
        if (length is null && width is null && height is null && string.IsNullOrWhiteSpace(unitOfMeasure))
        {
            return null;
        }

        if (length is null || width is null || height is null || string.IsNullOrWhiteSpace(unitOfMeasure))
        {
            throw new ArgumentException("Length, width, height, and a dimension unit must be supplied together.", nameof(unitOfMeasure));
        }

        if (length <= 0m || width <= 0m || height <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Dimensions must be greater than zero.");
        }

        var normalized = ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure);
        if (normalized is not ("M" or "CM" or "MM"))
        {
            throw new ArgumentException("Dimension unit must be M, CM, or MM.", nameof(unitOfMeasure));
        }

        return normalized;
    }

    private static decimal GetMetresPerUnit(string unitOfMeasure) => unitOfMeasure switch
    {
        "M" => 1m,
        "CM" => 0.01m,
        "MM" => 0.001m,
        _ => throw new InvalidOperationException($"Unsupported dimension unit '{unitOfMeasure}'.")
    };
}
