using Warehouse.Domain.Common;

namespace Warehouse.Domain.Products;

public sealed class ProductCategory : PersistentEntity
{
    private ProductCategory(
        Guid id,
        string code,
        string name,
        Guid? parentCategoryId,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        Code = code;
        Name = name;
        ParentCategoryId = parentCategoryId;
    }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public Guid? ParentCategoryId { get; private set; }

    public static ProductCategory Create(
        string? code,
        string? name,
        Guid? parentCategoryId,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        return new ProductCategory(
            Guid.NewGuid(),
            NormalizeCode(code),
            NormalizeName(name),
            parentCategoryId,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public static string NormalizeCode(string? code)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Category code is required.", nameof(code));
        }

        if (normalized.Length > ProductCategoryRules.MaxCodeLength)
        {
            throw new ArgumentException($"Category code cannot exceed {ProductCategoryRules.MaxCodeLength} characters.", nameof(code));
        }

        return normalized;
    }

    private static string NormalizeName(string? name)
    {
        var normalized = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        if (normalized.Length > ProductCategoryRules.MaxNameLength)
        {
            throw new ArgumentException($"Category name cannot exceed {ProductCategoryRules.MaxNameLength} characters.", nameof(name));
        }

        return normalized;
    }

    private static void EnsureUtc(DateTime timestamp, string parameterName)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.", parameterName);
        }
    }
}

public static class ProductCategoryRules
{
    public const int MaxCodeLength = 64;
    public const int MaxNameLength = 200;
}

