namespace Warehouse.Application.Receiving;

public sealed record GoodsReceiptLineInput(Guid PurchaseOrderLineId, decimal AcceptedQuantity);
public sealed record GoodsReceiptInput(Guid PurchaseOrderId, int PurchaseOrderVersion, DateTime ReceivedAtUtc, string? SupplierDeliveryNote, string? Notes, IReadOnlyCollection<GoodsReceiptLineInput> Lines);
public sealed record GoodsReceiptResponse(Guid Id, string Number, Guid PurchaseOrderId, Guid WarehouseId, DateTime ReceivedAtUtc, int PurchaseOrderVersion);
public sealed record GoodsReceiptCandidateResponse(Guid PurchaseOrderId, string PurchaseOrderNumber, Guid WarehouseId, string? CurrencyCode, int Version, IReadOnlyCollection<GoodsReceiptCandidateLineResponse> Lines);
public sealed record GoodsReceiptCandidateLineResponse(Guid PurchaseOrderLineId, int LineNumber, string ProductSku, string ProductName, string UnitOfMeasure, decimal OrderedQuantity, decimal ReceivedQuantity, decimal OutstandingQuantity);
