namespace Warehouse.Application.Sales;

public sealed class SalesOrderNotFoundException(Guid id) : Exception($"Sales order '{id}' was not found.");
public sealed class SalesOrderConcurrencyException(Guid id, Exception? innerException = null) : Exception($"Sales order '{id}' was changed by another user.", innerException);
public sealed class SalesOrderFieldValidationException(string propertyName, string errorCode, string message) : Exception(message)
{
    public string PropertyName { get; } = propertyName;
    public string ErrorCode { get; } = errorCode;
}
public sealed class SalesOrderImmutableException(Guid id) : Exception($"Sales order '{id}' cannot be changed in its current state.");
public sealed class SalesOrderInvalidTransitionException(Guid id) : Exception($"Sales order '{id}' cannot make that status change.");
