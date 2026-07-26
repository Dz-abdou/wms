using Warehouse.Domain.Common;

namespace Warehouse.Domain.Currencies;

public sealed class Currency : PersistentEntity
{
    private Currency(Guid id, string code, string name, string? symbol, int decimalPlaces, bool isActive, bool isDefault, DateTime createdAtUtc, DateTime updatedAtUtc, Guid? createdByUserId, Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        Code = code;
        Name = name;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
        IsActive = isActive;
        IsDefault = isDefault;
    }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Symbol { get; private set; }
    public int DecimalPlaces { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }

    public static Currency Create(string? code, string? name, string? symbol, int decimalPlaces, bool isDefault, DateTime createdAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(createdAtUtc);
        return new Currency(Guid.NewGuid(), NormalizeCode(code), NormalizeName(name), NormalizeSymbol(symbol), ValidateDecimalPlaces(decimalPlaces), true, isDefault, createdAtUtc, createdAtUtc, actorUserId, actorUserId);
    }

    public void Update(string? name, string? symbol, int decimalPlaces, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc);
        Name = NormalizeName(name);
        Symbol = NormalizeSymbol(symbol);
        DecimalPlaces = ValidateDecimalPlaces(decimalPlaces);
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void SetStatus(bool isActive, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc);
        if (IsDefault && !isActive) throw new InvalidOperationException("The default currency cannot be inactive.");
        if (IsActive == isActive) return;
        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void SetDefault(bool isDefault, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc);
        if (isDefault && !IsActive) throw new InvalidOperationException("An inactive currency cannot be default.");
        if (IsDefault == isDefault) return;
        IsDefault = isDefault;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public static string NormalizeCode(string? code)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != CurrencyRules.CodeLength || normalized.Any(character => !char.IsAsciiLetter(character))) throw new ArgumentException("Currency code must contain three letters.", nameof(code));
        return normalized;
    }

    private static string NormalizeName(string? name)
    {
        var normalized = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > CurrencyRules.MaxNameLength) throw new ArgumentException("Currency name is required and must be within the supported length.", nameof(name));
        return normalized;
    }

    private static string? NormalizeSymbol(string? symbol)
    {
        var normalized = symbol?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) return null;
        if (normalized.Length > CurrencyRules.MaxSymbolLength) throw new ArgumentException("Currency symbol exceeds the supported length.", nameof(symbol));
        return normalized;
    }

    private static int ValidateDecimalPlaces(int decimalPlaces) => decimalPlaces is >= 0 and <= CurrencyRules.MaxDecimalPlaces ? decimalPlaces : throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
    private static void EnsureUtc(DateTime timestamp) { if (timestamp.Kind != DateTimeKind.Utc) throw new ArgumentException("Timestamps must be UTC."); }
}

public static class CurrencyRules
{
    public const int CodeLength = 3;
    public const int MaxNameLength = 100;
    public const int MaxSymbolLength = 10;
    public const int MaxDecimalPlaces = 4;
}
