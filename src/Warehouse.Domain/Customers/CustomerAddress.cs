using Warehouse.Domain.Common;

namespace Warehouse.Domain.Customers;

public sealed class CustomerAddress : PersistentEntity
{
    private CustomerAddress(
        Guid id,
        Guid customerId,
        string label,
        string addressLine1,
        string? addressLine2,
        string city,
        string? postalCode,
        string countryCode,
        bool isShippingAddress,
        bool isBillingAddress,
        string? deliveryInstructions,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        Guid? createdByUserId,
        Guid? updatedByUserId)
        : base(id, createdAtUtc, updatedAtUtc, createdByUserId, updatedByUserId)
    {
        CustomerId = customerId;
        Label = label;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        PostalCode = postalCode;
        CountryCode = countryCode;
        IsShippingAddress = isShippingAddress;
        IsBillingAddress = isBillingAddress;
        DeliveryInstructions = deliveryInstructions;
    }

    public Guid CustomerId { get; private set; }

    public string Label { get; private set; } = null!;

    public string AddressLine1 { get; private set; } = null!;

    public string? AddressLine2 { get; private set; }

    public string City { get; private set; } = null!;

    public string? PostalCode { get; private set; }

    public string CountryCode { get; private set; } = null!;

    public bool IsShippingAddress { get; private set; }

    public bool IsBillingAddress { get; private set; }

    public string? DeliveryInstructions { get; private set; }

    public static CustomerAddress Create(
        Guid customerId,
        string? label,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? postalCode,
        string? countryCode,
        bool isShippingAddress,
        bool isBillingAddress,
        string? deliveryInstructions,
        DateTime createdAtUtc,
        Guid? actorUserId = null)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(nameof(customerId));
        }

        if (!isShippingAddress && !isBillingAddress)
        {
            throw new ArgumentException("A customer address must be shipping, billing, or both.");
        }

        Customer.EnsureUtc(createdAtUtc);
        return new CustomerAddress(
            Guid.NewGuid(),
            customerId,
            Customer.NormalizeRequired(label, CustomerAddressRules.MaxLabelLength, "Customer address label"),
            Customer.NormalizeRequired(addressLine1, CustomerAddressRules.MaxAddressLineLength, "Customer address line 1"),
            Customer.NormalizeOptional(addressLine2, CustomerAddressRules.MaxAddressLineLength, "Customer address line 2"),
            Customer.NormalizeRequired(city, CustomerAddressRules.MaxCityLength, "Customer address city"),
            Customer.NormalizeOptional(postalCode, CustomerAddressRules.MaxPostalCodeLength, "Customer address postal code"),
            NormalizeCountryCode(countryCode),
            isShippingAddress,
            isBillingAddress,
            Customer.NormalizeOptional(deliveryInstructions, CustomerAddressRules.MaxDeliveryInstructionsLength, "Customer address delivery instructions"),
            createdAtUtc,
            createdAtUtc,
            actorUserId,
            actorUserId);
    }

    public void Update(
        string? label,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? postalCode,
        string? countryCode,
        bool isShippingAddress,
        bool isBillingAddress,
        string? deliveryInstructions,
        DateTime updatedAtUtc,
        Guid? actorUserId = null)
    {
        if (!isShippingAddress && !isBillingAddress)
        {
            throw new ArgumentException("A customer address must be shipping, billing, or both.");
        }

        Customer.EnsureUtc(updatedAtUtc);
        var normalizedLabel = Customer.NormalizeRequired(label, CustomerAddressRules.MaxLabelLength, "Customer address label");
        var normalizedAddressLine1 = Customer.NormalizeRequired(addressLine1, CustomerAddressRules.MaxAddressLineLength, "Customer address line 1");
        var normalizedAddressLine2 = Customer.NormalizeOptional(addressLine2, CustomerAddressRules.MaxAddressLineLength, "Customer address line 2");
        var normalizedCity = Customer.NormalizeRequired(city, CustomerAddressRules.MaxCityLength, "Customer address city");
        var normalizedPostalCode = Customer.NormalizeOptional(postalCode, CustomerAddressRules.MaxPostalCodeLength, "Customer address postal code");
        var normalizedCountryCode = NormalizeCountryCode(countryCode);
        var normalizedDeliveryInstructions = Customer.NormalizeOptional(deliveryInstructions, CustomerAddressRules.MaxDeliveryInstructionsLength, "Customer address delivery instructions");

        if (Label == normalizedLabel &&
            AddressLine1 == normalizedAddressLine1 &&
            AddressLine2 == normalizedAddressLine2 &&
            City == normalizedCity &&
            PostalCode == normalizedPostalCode &&
            CountryCode == normalizedCountryCode &&
            IsShippingAddress == isShippingAddress &&
            IsBillingAddress == isBillingAddress &&
            DeliveryInstructions == normalizedDeliveryInstructions)
        {
            return;
        }

        Label = normalizedLabel;
        AddressLine1 = normalizedAddressLine1;
        AddressLine2 = normalizedAddressLine2;
        City = normalizedCity;
        PostalCode = normalizedPostalCode;
        CountryCode = normalizedCountryCode;
        IsShippingAddress = isShippingAddress;
        IsBillingAddress = isBillingAddress;
        DeliveryInstructions = normalizedDeliveryInstructions;
        UpdatedAtUtc = updatedAtUtc;
        SetUpdatedByUser(actorUserId);
    }

    private static string NormalizeCountryCode(string? countryCode)
    {
        var normalized = countryCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.Length != CustomerAddressRules.CountryCodeLength ||
            normalized.Any(character => !char.IsAsciiLetter(character)))
        {
            throw new ArgumentException("Customer address country code must be a two-letter ISO code.", nameof(countryCode));
        }

        return normalized;
    }
}

public static class CustomerAddressRules
{
    public const int MaxLabelLength = 100;
    public const int MaxAddressLineLength = 200;
    public const int MaxCityLength = 100;
    public const int MaxPostalCodeLength = 32;
    public const int CountryCodeLength = 2;
    public const int MaxDeliveryInstructionsLength = 1000;
}
