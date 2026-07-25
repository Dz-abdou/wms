using Microsoft.Extensions.Options;
using Warehouse.Application.Purchasing;

namespace Warehouse.Infrastructure.Purchasing;

public sealed class PurchasingCurrencyOptions
{
    public const string SectionName = "Purchasing:Currency";

    public string? DefaultCode { get; init; }

    public IReadOnlyCollection<string>? AllowedCodes { get; init; }
}

public sealed class ConfigurationCurrencyCatalogue : ICurrencyCatalogue
{
    private readonly IReadOnlyCollection<CurrencyOption> options;
    private readonly HashSet<string> codes;

    public ConfigurationCurrencyCatalogue(IOptions<PurchasingCurrencyOptions> configuration)
    {
        var configuredOptions = configuration.Value;
        var defaultCode = Normalize(configuredOptions.DefaultCode, nameof(configuredOptions.DefaultCode));
        var normalizedCodes = (configuredOptions.AllowedCodes ?? [])
            .Select(code => Normalize(code, nameof(configuredOptions.AllowedCodes)))
            .Distinct(StringComparer.Ordinal)
            .Order()
            .ToArray();

        if (normalizedCodes.Length == 0 || !normalizedCodes.Contains(defaultCode, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("Purchasing currency configuration must contain the default currency in AllowedCodes.");
        }

        codes = normalizedCodes.ToHashSet(StringComparer.Ordinal);
        options = normalizedCodes
            .Select(code => new CurrencyOption(code, code == defaultCode))
            .ToArray();
    }

    public IReadOnlyCollection<CurrencyOption> GetOptions() => options;

    public bool IsSupported(string currencyCode) => codes.Contains(currencyCode);

    private static string Normalize(string? code, string parameterName)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != 3 || normalized.Any(character => !char.IsAsciiLetter(character)))
        {
            throw new InvalidOperationException($"{parameterName} must contain three-letter ISO currency codes.");
        }

        return normalized;
    }
}
