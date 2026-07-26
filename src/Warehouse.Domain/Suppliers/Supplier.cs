using Warehouse.Domain.Common;

namespace Warehouse.Domain.Suppliers;

public sealed class Supplier : PersistentEntity
{
    private Supplier(Guid id, string code, string name, string? email, string? phoneNumber, string? address, string defaultCurrencyCode, bool isActive, DateTime createdAtUtc, DateTime updatedAtUtc, Guid? createdByUserId, Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        Code = code;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        DefaultCurrencyCode = defaultCurrencyCode;
        IsActive = isActive;
    }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public string DefaultCurrencyCode { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public static Supplier Create(string? code, string? name, string? email, string? phoneNumber, string? address, DateTime createdAtUtc, Guid? actorUserId = null, string? defaultCurrencyCode = SupplierRules.DefaultCurrencyCode)
    {
        EnsureUtc(createdAtUtc);
        return new Supplier(Guid.NewGuid(), NormalizeCode(code), NormalizeRequired(name, SupplierRules.MaxNameLength, "Supplier name"), NormalizeOptional(email, SupplierRules.MaxEmailLength, "Supplier email"), NormalizeOptional(phoneNumber, SupplierRules.MaxPhoneNumberLength, "Supplier phone number"), NormalizeOptional(address, SupplierRules.MaxAddressLength, "Supplier address"), NormalizeCurrencyCode(defaultCurrencyCode), true, createdAtUtc, createdAtUtc, actorUserId, actorUserId);
    }

    public void Update(string? code, string? name, string? email, string? phoneNumber, string? address, DateTime updatedAtUtc, Guid? actorUserId = null, string? defaultCurrencyCode = SupplierRules.DefaultCurrencyCode)
    {
        EnsureUtc(updatedAtUtc);
        var normalizedCode = NormalizeCode(code);
        var normalizedName = NormalizeRequired(name, SupplierRules.MaxNameLength, "Supplier name");
        var normalizedEmail = NormalizeOptional(email, SupplierRules.MaxEmailLength, "Supplier email");
        var normalizedPhoneNumber = NormalizeOptional(phoneNumber, SupplierRules.MaxPhoneNumberLength, "Supplier phone number");
        var normalizedAddress = NormalizeOptional(address, SupplierRules.MaxAddressLength, "Supplier address");
        var normalizedDefaultCurrencyCode = NormalizeCurrencyCode(defaultCurrencyCode);
        if (Code == normalizedCode && Name == normalizedName && Email == normalizedEmail && PhoneNumber == normalizedPhoneNumber && Address == normalizedAddress && DefaultCurrencyCode == normalizedDefaultCurrencyCode) return;
        Code = normalizedCode;
        Name = normalizedName;
        Email = normalizedEmail;
        PhoneNumber = normalizedPhoneNumber;
        Address = normalizedAddress;
        DefaultCurrencyCode = normalizedDefaultCurrencyCode;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void SetStatus(bool isActive, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc);
        if (IsActive == isActive) return;
        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public static string NormalizeCode(string? code)
    {
        var trimmed = code?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) throw new ArgumentException("Supplier code is required.", nameof(code));
        if (trimmed.Length > SupplierRules.MaxCodeLength) throw new ArgumentException($"Supplier code cannot exceed {SupplierRules.MaxCodeLength} characters.", nameof(code));
        return trimmed.ToUpperInvariant();
    }

    public static string NormalizeCurrencyCode(string? currencyCode)
    {
        var normalized = currencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != SupplierRules.CurrencyCodeLength || normalized.Any(character => !char.IsAsciiLetter(character)))
        {
            throw new ArgumentException("Supplier default currency must be a three-letter ISO code.", nameof(currencyCode));
        }

        return normalized;
    }

    private static string NormalizeRequired(string? value, int maximumLength, string label)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) throw new ArgumentException($"{label} is required.");
        if (trimmed.Length > maximumLength) throw new ArgumentException($"{label} cannot exceed {maximumLength} characters.");
        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maximumLength, string label)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return null;
        if (trimmed.Length > maximumLength) throw new ArgumentException($"{label} cannot exceed {maximumLength} characters.");
        return trimmed;
    }

    private static void EnsureUtc(DateTime timestamp)
    {
        if (timestamp.Kind != DateTimeKind.Utc) throw new ArgumentException("Timestamps must be UTC.");
    }
}
