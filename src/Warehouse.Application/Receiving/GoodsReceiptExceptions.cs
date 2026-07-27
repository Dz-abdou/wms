namespace Warehouse.Application.Receiving;
public sealed class GoodsReceiptPurchaseOrderUnavailableException : Exception { public GoodsReceiptPurchaseOrderUnavailableException() : base("The purchase order cannot be received.") { } }
public sealed class GoodsReceiptConcurrencyException : Exception { public GoodsReceiptConcurrencyException() : base("The purchase order changed while this receipt was being posted.") { } }
public sealed class GoodsReceiptOverReceiptException(int lineIndex) : Exception("Accepted quantity exceeds the outstanding purchase-order quantity.") { public string PropertyName => $"Lines[{lineIndex}].AcceptedQuantity"; }
