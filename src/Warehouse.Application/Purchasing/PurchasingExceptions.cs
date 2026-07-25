namespace Warehouse.Application.Purchasing;

public sealed class SupplierProductNotFoundException(Guid supplierProductId)
    : Exception($"Supplier catalogue item '{supplierProductId}' was not found.");

public sealed class SupplierProductConflictException(Guid supplierId, Guid productId, string unitOfMeasure, Exception? innerException = null)
    : Exception($"Supplier '{supplierId}' already has a catalogue item for product '{productId}' in unit '{unitOfMeasure}'.", innerException);

public sealed class PurchaseOrderNotFoundException(Guid purchaseOrderId)
    : Exception($"Purchase order '{purchaseOrderId}' was not found.");

public sealed class PurchaseOrderImmutableException(Guid purchaseOrderId)
    : Exception($"Purchase order '{purchaseOrderId}' is no longer a draft.");

public sealed class PurchaseOrderCatalogueInvalidException(string message) : Exception(message);

public sealed class PurchaseOrderSubmissionInvalidException(string message) : Exception(message);
