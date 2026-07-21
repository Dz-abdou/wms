using Warehouse.Domain.Auditing;

namespace Warehouse.Domain.Products;

public sealed class Product : AuditableEntity
{
    private Product(
        Guid id,
        string sku,
        string name,
        string? description,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Sku { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Product Create(string? sku, string? name, string? description, DateTime createdAtUtc)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        return new Product(
            Guid.NewGuid(),
            NormalizeSku(sku),
            NormalizeName(name),
            NormalizeDescription(description),
            true,
            createdAtUtc,
            createdAtUtc);
    }

    public void Update(string? sku, string? name, string? description, DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        Sku = NormalizeSku(sku);
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SetStatus(bool isActive, DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
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