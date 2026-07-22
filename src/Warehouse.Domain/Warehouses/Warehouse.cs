using Warehouse.Domain.Common;

namespace Warehouse.Domain.Warehouses;

public sealed class Warehouse : PersistentEntity
{
    private Warehouse(
        Guid id,
        string code,
        string name,
        string? description,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        Code = code;
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public static Warehouse Create(string? code, string? name, string? description, DateTime createdAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        return new Warehouse(
            Guid.NewGuid(),
            NormalizeCode(code),
            NormalizeName(name),
            NormalizeDescription(description),
            true,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public void Update(string? code, string? name, string? description, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        var normalizedCode = NormalizeCode(code);
        var normalizedName = NormalizeName(name);
        var normalizedDescription = NormalizeDescription(description);
        if (Code == normalizedCode && Name == normalizedName && Description == normalizedDescription)
        {
            return;
        }

        Code = normalizedCode;
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

    public static string NormalizeCode(string? code)
    {
        var trimmedCode = code?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedCode))
        {
            throw new ArgumentException("Warehouse code is required.", nameof(code));
        }

        if (trimmedCode.Length > WarehouseRules.MaxCodeLength)
        {
            throw new ArgumentException($"Warehouse code cannot exceed {WarehouseRules.MaxCodeLength} characters.", nameof(code));
        }

        return trimmedCode.ToUpperInvariant();
    }

    private static string NormalizeName(string? name)
    {
        var trimmedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ArgumentException("Warehouse name is required.", nameof(name));
        }

        if (trimmedName.Length > WarehouseRules.MaxNameLength)
        {
            throw new ArgumentException($"Warehouse name cannot exceed {WarehouseRules.MaxNameLength} characters.", nameof(name));
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

        if (trimmedDescription.Length > WarehouseRules.MaxDescriptionLength)
        {
            throw new ArgumentException($"Warehouse description cannot exceed {WarehouseRules.MaxDescriptionLength} characters.", nameof(description));
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