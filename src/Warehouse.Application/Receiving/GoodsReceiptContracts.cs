namespace Warehouse.Application.Receiving;

public sealed record GoodsReceiptLineInput(Guid PurchaseOrderLineId, decimal AcceptedQuantity);
public sealed record GoodsReceiptInput(Guid PurchaseOrderId, int PurchaseOrderVersion, DateTime ReceivedAtUtc, string? SupplierDeliveryNote, string? Notes, IReadOnlyCollection<GoodsReceiptLineInput> Lines);
public sealed record GoodsReceiptResponse(Guid Id, string Number, Guid PurchaseOrderId, Guid WarehouseId, DateTime ReceivedAtUtc, int PurchaseOrderVersion);
