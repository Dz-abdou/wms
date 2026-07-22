using Warehouse.Domain.Common;

namespace Warehouse.Domain.Products;

public sealed class Product : PersistentEntity
{
    private readonly List<ProductUnitConversion> unitConversions = [];

    private Product(
        Guid id,
        string sku,
        string name,
        string? description,
        string baseUnitOfMeasure,
        Guid? categoryId,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        Sku = sku;
        Name = name;
        Description = description;
        BaseUnitOfMeasure = baseUnitOfMeasure;
        CategoryId = categoryId;
        IsActive = isActive;
    }

    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string BaseUnitOfMeasure { get; private set; } = null!;
    public Guid? CategoryId { get; private set; }
    public IReadOnlyCollection<ProductUnitConversion> UnitConversions => unitConversions;
    public ProductMeasurements? Measurements { get; private set; }
    public bool IsActive { get; private set; }

    public static Product Create(string? sku, string? name, string? description, DateTime createdAtUtc, Guid? actorUserId = null) =>
        Create(sku, name, description, "EA", [], null, createdAtUtc, actorUserId);

    public static Product Create(
        string? sku,
        string? name,
        string? description,
        string? baseUnitOfMeasure,
        IEnumerable<ProductUnitConversionDefinition> unitConversionDefinitions,
        ProductMeasurements? measurements,
        DateTime createdAtUtc,
        Guid? actorUserId = null,
        Guid? categoryId = null)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        var product = new Product(
            Guid.NewGuid(),
            NormalizeSku(sku),
            NormalizeName(name),
            NormalizeDescription(description),
            ProductUnitOfMeasure.NormalizeBaseUnitOfMeasure(baseUnitOfMeasure),
            categoryId,
            true,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
        product.ApplyCatalogueDetails(baseUnitOfMeasure, unitConversionDefinitions, measurements);
        return product;
    }

    public void Update(string? sku, string? name, string? description, DateTime updatedAtUtc, Guid? actorUserId = null) =>
        Update(
            sku,
            name,
            description,
            BaseUnitOfMeasure,
            UnitConversions.Select(conversion => new ProductUnitConversionDefinition(conversion.UnitOfMeasure, conversion.QuantityInBaseUnit)),
            Measurements,
            updatedAtUtc,
            actorUserId,
            CategoryId);

    public void Update(
        string? sku,
        string? name,
        string? description,
        string? baseUnitOfMeasure,
        IEnumerable<ProductUnitConversionDefinition> unitConversionDefinitions,
        ProductMeasurements? measurements,
        DateTime updatedAtUtc,
        Guid? actorUserId = null,
        Guid? categoryId = null)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        var normalizedSku = NormalizeSku(sku);
        var normalizedName = NormalizeName(name);
        var normalizedDescription = NormalizeDescription(description);
        var normalizedBaseUnit = ProductUnitOfMeasure.NormalizeBaseUnitOfMeasure(baseUnitOfMeasure);
        var normalizedConversions = NormalizeConversions(normalizedBaseUnit, unitConversionDefinitions);

        if (Sku == normalizedSku && Name == normalizedName && Description == normalizedDescription &&
            BaseUnitOfMeasure == normalizedBaseUnit && CategoryId == categoryId && HasSameConversions(normalizedConversions) &&
            (Measurements?.IsSameAs(measurements) ?? measurements is null))
        {
            return;
        }

        Sku = normalizedSku;
        Name = normalizedName;
        Description = normalizedDescription;
        BaseUnitOfMeasure = normalizedBaseUnit;
        CategoryId = categoryId;
        ReplaceConversions(normalizedConversions);
        Measurements = measurements;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void SetStatus(bool isActive, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public bool TryConvertToBaseQuantity(string? unitOfMeasure, decimal quantity, out decimal quantityInBaseUnit)
    {
        quantityInBaseUnit = 0m;
        if (quantity <= 0m)
        {
            return false;
        }

        try
        {
            var normalizedUnit = ProductUnitOfMeasure.NormalizeUnitOfMeasure(unitOfMeasure);
            if (normalizedUnit == BaseUnitOfMeasure)
            {
                quantityInBaseUnit = quantity;
                return true;
            }

            var conversion = UnitConversions.SingleOrDefault(candidate => candidate.UnitOfMeasure == normalizedUnit);
            if (conversion is null)
            {
                return false;
            }

            quantityInBaseUnit = quantity * conversion.QuantityInBaseUnit;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static string NormalizeSku(string? sku)
    {
        var trimmedSku = sku?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedSku))
        {
            throw new ArgumentException("SKU is required.", nameof(sku));
        }

        if (trimmedSku.Length > ProductRules.MaxSkuLength)
        {
            throw new ArgumentException($"SKU cannot exceed {ProductRules.MaxSkuLength} characters.", nameof(sku));
        }

        return trimmedSku.ToUpperInvariant();
    }

    private void ApplyCatalogueDetails(
        string? baseUnitOfMeasure,
        IEnumerable<ProductUnitConversionDefinition> unitConversionDefinitions,
        ProductMeasurements? measurements)
    {
        BaseUnitOfMeasure = ProductUnitOfMeasure.NormalizeBaseUnitOfMeasure(baseUnitOfMeasure);
        ReplaceConversions(NormalizeConversions(BaseUnitOfMeasure, unitConversionDefinitions));
        Measurements = measurements;
    }

    private static IReadOnlyList<ProductUnitConversion> NormalizeConversions(
        string baseUnitOfMeasure,
        IEnumerable<ProductUnitConversionDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        var conversions = definitions
            .Select(definition => ProductUnitConversion.Create(definition.UnitOfMeasure, definition.QuantityInBaseUnit))
            .OrderBy(conversion => conversion.UnitOfMeasure)
            .ToList();

        if (conversions.Any(conversion => conversion.UnitOfMeasure == baseUnitOfMeasure))
        {
            throw new ArgumentException("The base unit of measure must not be repeated as a conversion.", nameof(definitions));
        }

        if (conversions.Select(conversion => conversion.UnitOfMeasure).Distinct().Count() != conversions.Count)
        {
            throw new ArgumentException("Each conversion unit of measure must be unique.", nameof(definitions));
        }

        return conversions;
    }

    private void ReplaceConversions(IEnumerable<ProductUnitConversion> conversions)
    {
        unitConversions.Clear();
        unitConversions.AddRange(conversions);
    }

    private bool HasSameConversions(IReadOnlyList<ProductUnitConversion> conversions) =>
        UnitConversions.Count == conversions.Count &&
        UnitConversions.OrderBy(conversion => conversion.UnitOfMeasure)
            .Zip(conversions, (current, candidate) =>
                current.UnitOfMeasure == candidate.UnitOfMeasure &&
                current.QuantityInBaseUnit == candidate.QuantityInBaseUnit)
            .All(isSame => isSame);

    private static string NormalizeName(string? name)
    {
        var trimmedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (trimmedName.Length > ProductRules.MaxNameLength)
        {
            throw new ArgumentException($"Name cannot exceed {ProductRules.MaxNameLength} characters.", nameof(name));
        }

        return trimmedName;
    }

    private static string? NormalizeDescription(string? description)
    {
        var trimmedDescription = description?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedDescription))
        {
            return null;
        }

        if (trimmedDescription.Length > ProductRules.MaxDescriptionLength)
        {
            throw new ArgumentException($"Description cannot exceed {ProductRules.MaxDescriptionLength} characters.", nameof(description));
        }

        return trimmedDescription;
    }

    private static void EnsureUtc(DateTime timestamp, string parameterName)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.", parameterName);
        }
    }
}
