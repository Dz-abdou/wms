namespace Warehouse.Domain.Warehouses;

public sealed class Warehouse
{
    private Warehouse(
        Guid id,
        string code,
        string name,
        string? description,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Warehouse Create(string? code, string? name, string? description, DateTime createdAtUtc)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        return new Warehouse(
            Guid.NewGuid(),
            NormalizeCode(code),
            NormalizeName(name),
            NormalizeDescription(description),
            true,
            createdAtUtc,
            createdAtUtc);
    }

    public void Update(string? code, string? name, string? description, DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        Code = NormalizeCode(code);
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