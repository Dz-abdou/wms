export const receivingApiPaths = {
  goodsReceipts: "/api/goods-receipts",
  goodsReceiptById: (id: string) => `/api/goods-receipts/${id}`,
  receiptCandidate: (purchaseOrderId: string) =>
    `/api/purchase-orders/${purchaseOrderId}/receipt-candidate`,
} as const;

export const receivingRoutes = {
  list: "/goods-receipts",
  listPattern: "goods-receipts",
  create: (purchaseOrderId: string) => `/goods-receipts/new/${purchaseOrderId}`,
  createPattern: "goods-receipts/new/:purchaseOrderId",
  detail: (id: string) => `/goods-receipts/${id}`,
  detailPattern: "goods-receipts/:id",
} as const;

export const receivingValidation = {
  maxSupplierDeliveryNoteLength: 128,
  maxNotesLength: 2000,
  acceptedQuantityPrecision: 6,
} as const;
