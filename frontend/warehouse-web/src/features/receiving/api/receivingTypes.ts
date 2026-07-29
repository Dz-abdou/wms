export type GoodsReceiptLineInput = {
  purchaseOrderLineId: string;
  acceptedQuantity: number;
};

export type GoodsReceiptInput = {
  purchaseOrderId: string;
  purchaseOrderVersion: number;
  receivedAtUtc: string;
  supplierDeliveryNote?: string;
  notes?: string;
  lines: GoodsReceiptLineInput[];
};

export type GoodsReceipt = {
  id: string;
  number: string;
  purchaseOrderId: string;
  warehouseId: string;
  receivedAtUtc: string;
  purchaseOrderVersion: number;
};

export type GoodsReceiptCandidateLine = {
  purchaseOrderLineId: string;
  lineNumber: number;
  productSku: string;
  productName: string;
  unitOfMeasure: string;
  orderedQuantity: number;
  receivedQuantity: number;
  outstandingQuantity: number;
  conversionFactorToBaseUnit: number;
};

export type GoodsReceiptCandidate = {
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  currencyCode: string | null;
  version: number;
  lines: GoodsReceiptCandidateLine[];
};

export type GoodsReceiptListQuery = {
  page: number;
  pageSize: number;
  purchaseOrderNumber?: string;
  warehouseId?: string;
};

export type GoodsReceiptListItem = {
  id: string;
  number: string;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  receivedAtUtc: string;
  lineCount: number;
};

export type GoodsReceiptListResult = {
  items: GoodsReceiptListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type GoodsReceiptLine = {
  id: string;
  purchaseOrderLineId: string;
  purchaseOrderLineNumber: number;
  productId: string;
  productSku: string;
  productName: string;
  unitOfMeasure: string;
  acceptedQuantity: number;
  acceptedQuantityInBaseUnit: number;
  conversionFactorToBaseUnit: number;
  inventoryMovementId: string;
};

export type GoodsReceiptDetail = {
  id: string;
  number: string;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  receivedAtUtc: string;
  supplierDeliveryNote: string | null;
  notes: string | null;
  receiverUserId: string;
  createdAtUtc: string;
  lines: GoodsReceiptLine[];
};
