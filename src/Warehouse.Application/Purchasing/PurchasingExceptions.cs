namespace Warehouse.Application.Purchasing;

public sealed class SupplierProductNotFoundException(Guid supplierProductId)
    : Exception($"Supplier catalogue item '{supplierProductId}' was not found.");

public sealed class SupplierProductConflictException(Guid supplierId, Guid productId, string unitOfMeasure, Exception? innerException = null)
    : Exception($"Supplier '{supplierId}' already has a catalogue item for product '{productId}' in unit '{unitOfMeasure}'.", innerException);

public sealed class SupplierProductCurrencyNotSupportedException(string currencyCode)
    : Exception($"Currency '{currencyCode}' is not enabled for purchasing.");

public sealed class PurchaseOrderNotFoundException(Guid purchaseOrderId)
    : Exception($"Purchase order '{purchaseOrderId}' was not found.");

public sealed class PurchaseOrderImmutableException(Guid purchaseOrderId)
    : Exception($"Purchase order '{purchaseOrderId}' is no longer a draft.");

public sealed class PurchaseOrderCatalogueInvalidException(string message) : Exception(message);

public sealed class PurchaseOrderMinimumOrderQuantityException(int lineIndex, decimal minimumOrderQuantity)
    : Exception($"Purchase-order line {lineIndex + 1} must have a quantity of at least {minimumOrderQuantity}.")
{
    public string PropertyName => $"Lines[{lineIndex}].Quantity";
}

public sealed class PurchaseOrderSubmissionInvalidException(string message) : Exception(message);

public sealed class PurchaseOrderConcurrencyException(Guid purchaseOrderId, Exception? innerException = null)
    : Exception($"Purchase order '{purchaseOrderId}' was changed by another user.", innerException);

public sealed class PurchaseOrderInvalidTransitionException(Guid purchaseOrderId)
    : Exception($"Purchase order '{purchaseOrderId}' cannot make that status transition.");
