namespace Warehouse.Application.Currencies;

public sealed class CurrencyNotFoundException(Guid id) : Exception($"Currency '{id}' was not found.");
public sealed class CurrencyCodeConflictException(string code, Exception? innerException = null) : Exception($"Currency code '{code}' already exists.", innerException);
public sealed class DefaultCurrencyRequiredException : Exception
{
    public DefaultCurrencyRequiredException() : base("Choose another active default currency before deactivating the current default.") { }
}
public sealed class InactiveCurrencyCannotBeDefaultException(Guid id) : Exception($"Currency '{id}' must be active before it can be default.");
