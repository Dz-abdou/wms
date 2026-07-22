using Warehouse.Domain.Common;

namespace Warehouse.Domain.Products;

public sealed class Product : PersistentEntity
{
    private Product(
        Guid id,
        string sku,
        string name,
        string? description,
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
        IsActive = isActive;
    }

    public string Sku { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public static Product Create(string? sku, string? name, string? description, DateTime createdAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        return new Product(
            Guid.NewGuid(),
            NormalizeSku(sku),
            NormalizeName(name),
            NormalizeDescription(description),
            true,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public void Update(string? sku, string? name, string? description, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        var normalizedSku = NormalizeSku(sku);
        var normalizedName = NormalizeName(name);
        var normalizedDescription = NormalizeDescription(description);
        if (Sku == normalizedSku && Name == normalizedName && Description == normalizedDescription)
        {
            return;
        }

        Sku = normalizedSku;
        Name = normalizedName;
        Description = normalizedDescription;
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