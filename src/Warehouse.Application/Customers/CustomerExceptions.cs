namespace Warehouse.Application.Customers;

public sealed class CustomerNotFoundException(Guid customerId) : Exception($"Customer '{customerId}' was not found.");

public sealed class CustomerCodeConflictException(string code, Exception? innerException = null)
    : Exception($"A customer with code '{code}' already exists.", innerException);

public sealed class CustomerDefaultCurrencyNotSupportedException(string currencyCode)
    : Exception($"Customer default currency '{currencyCode}' is not active.");

public sealed class CustomerContactNotFoundException(Guid contactId) : Exception($"Customer contact '{contactId}' was not found.");

public sealed class CustomerAddressNotFoundException(Guid addressId) : Exception($"Customer address '{addressId}' was not found.");
