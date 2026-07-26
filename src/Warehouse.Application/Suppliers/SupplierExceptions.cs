namespace Warehouse.Application.Suppliers;

public sealed class SupplierNotFoundException(Guid supplierId) : Exception($"Supplier '{supplierId}' was not found.");

public sealed class SupplierCodeConflictException(string code, Exception? innerException = null)
    : Exception($"A supplier with code '{code}' already exists.", innerException);
