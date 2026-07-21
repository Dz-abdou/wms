namespace Warehouse.Application.Products;

public sealed class ProductNotFoundException(Guid productId)
    : Exception($"Product '{productId}' was not found.");

public sealed class ProductSkuConflictException(string sku, Exception? innerException = null)
    : Exception($"A product with SKU '{sku}' already exists.", innerException);