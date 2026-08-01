using Warehouse.Domain.Common;

namespace Warehouse.Domain.Customers;

public sealed class Customer : PersistentEntity
{
    private Customer(
        Guid id,
        string code,
        string legalName,
        string? tradingName,
        string? defaultCurrencyCode,
        string? deliveryInstructions,
        string? serviceNotes,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        Code = code;
        LegalName = legalName;
        TradingName = tradingName;
        DefaultCurrencyCode = defaultCurrencyCode;
        DeliveryInstructions = deliveryInstructions;
        ServiceNotes = serviceNotes;
        IsActive = isActive;
    }

    public string Code { get; private set; } = null!;

    public string LegalName { get; private set; } = null!;

    public string? TradingName { get; private set; }

    public string? DefaultCurrencyCode { get; private set; }

    public string? DeliveryInstructions { get; private set; }

    public string? ServiceNotes { get; private set; }

    public bool IsActive { get; private set; }

    public static Customer Create(
        string? code,
        string? legalName,
        string? tradingName,
        string? defaultCurrencyCode,
        string? deliveryInstructions,
        string? serviceNotes,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        EnsureUtc(createdAtUtc);
        return new Customer(
            Guid.NewGuid(),
            NormalizeCode(code),
            NormalizeRequired(legalName, CustomerRules.MaxLegalNameLength, "Customer legal name"),
            NormalizeOptional(tradingName, CustomerRules.MaxTradingNameLength, "Customer trading name"),
            NormalizeOptionalCurrencyCode(defaultCurrencyCode),
            NormalizeOptional(deliveryInstructions, CustomerRules.MaxDeliveryInstructionsLength, "Delivery instructions"),
            NormalizeOptional(serviceNotes, CustomerRules.MaxServiceNotesLength, "Service notes"),
            true,
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public void Update(
        string? code,
        string? legalName,
        string? tradingName,
        string? defaultCurrencyCode,
        string? deliveryInstructions,
        string? serviceNotes,
        DateTime updatedAtUtc,
        Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc);
        var normalizedCode = NormalizeCode(code);
        var normalizedLegalName = NormalizeRequired(legalName, CustomerRules.MaxLegalNameLength, "Customer legal name");
        var normalizedTradingName = NormalizeOptional(tradingName, CustomerRules.MaxTradingNameLength, "Customer trading name");
        var normalizedCurrencyCode = NormalizeOptionalCurrencyCode(defaultCurrencyCode);
        var normalizedDeliveryInstructions = NormalizeOptional(deliveryInstructions, CustomerRules.MaxDeliveryInstructionsLength, "Delivery instructions");
        var normalizedServiceNotes = NormalizeOptional(serviceNotes, CustomerRules.MaxServiceNotesLength, "Service notes");

        if (Code == normalizedCode &&
            LegalName == normalizedLegalName &&
            TradingName == normalizedTradingName &&
            DefaultCurrencyCode == normalizedCurrencyCode &&
            DeliveryInstructions == normalizedDeliveryInstructions &&
            ServiceNotes == normalizedServiceNotes)
        {
            return;
        }

        Code = normalizedCode;
        LegalName = normalizedLegalName;
        TradingName = normalizedTradingName;
        DefaultCurrencyCode = normalizedCurrencyCode;
        DeliveryInstructions = normalizedDeliveryInstructions;
        ServiceNotes = normalizedServiceNotes;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    public void SetStatus(bool isActive, DateTime updatedAtUtc, Guid? actorUserId = null)
    {
        EnsureUtc(updatedAtUtc);
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
        var trimmed = code?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("Customer code is required.", nameof(code));
        }

        if (trimmed.Length > CustomerRules.MaxCodeLength)
        {
            throw new ArgumentException($"Customer code cannot exceed {CustomerRules.MaxCodeLength} characters.", nameof(code));
        }

        return trimmed.ToUpperInvariant();
    }

    public static string? NormalizeOptionalCurrencyCode(string? currencyCode)
    {
        var normalized = currencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length != CustomerRules.CurrencyCodeLength || normalized.Any(character => !char.IsAsciiLetter(character)))
        {
            throw new ArgumentException("Customer default currency must be a three-letter ISO code.", nameof(currencyCode));
        }

        return normalized;
    }

    internal static string NormalizeRequired(string? value, int maximumLength, string label)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException($"{label} is required.");
        }

        if (trimmed.Length > maximumLength)
        {
            throw new ArgumentException($"{label} cannot exceed {maximumLength} characters.");
        }

        return trimmed;
    }

    internal static string? NormalizeOptional(string? value, int maximumLength, string label)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maximumLength)
        {
            throw new ArgumentException($"{label} cannot exceed {maximumLength} characters.");
        }

        return trimmed;
    }

    internal static void EnsureUtc(DateTime timestamp)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", nameof(timestamp));
        }
    }
}

public static class CustomerRules
{
    public const int MaxCodeLength = 32;
    public const int MaxLegalNameLength = 200;
    public const int MaxTradingNameLength = 200;
    public const int CurrencyCodeLength = 3;
    public const int MaxDeliveryInstructionsLength = 1000;
    public const int MaxServiceNotesLength = 1000;
}
