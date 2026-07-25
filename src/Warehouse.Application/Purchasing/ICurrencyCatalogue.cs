namespace Warehouse.Application.Purchasing;

public interface ICurrencyCatalogue
{
    IReadOnlyCollection<CurrencyOption> GetOptions();

    bool IsSupported(string currencyCode);
}

public sealed record CurrencyOption(string Code, bool IsDefault);
