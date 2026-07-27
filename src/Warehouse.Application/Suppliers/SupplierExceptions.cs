namespace Warehouse.Application.Suppliers;

public sealed class SupplierNotFoundException(Guid supplierId) : Exception($"Supplier '{supplierId}' was not found.");

public sealed class SupplierCodeConflictException(string code, Exception? innerException = null)
    : Exception($"A supplier with code '{code}' already exists.", innerException);

public sealed class SupplierDefaultCurrencyNotSupportedException(string currencyCode)
    : Exception($"Supplier default currency '{currencyCode}' is not active.");
