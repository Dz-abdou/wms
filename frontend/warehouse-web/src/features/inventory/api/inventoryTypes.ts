export type InventoryAdjustmentDirection = "Increase" | "Decrease";

export type InventoryAdjustmentInput = {
  reason:
    "StockCorrection" | "Damage" | "WriteOff" | "FoundStock" | "InitialBalance";
  reference?: string;
  note?: string;
  lines: InventoryAdjustmentLineInput[];
};

export type InventoryAdjustmentLineInput = {
  productId: string;
  warehouseId: string;
  quantity: number;
  direction: InventoryAdjustmentDirection;
  unitOfMeasure: string;
};

export type InventoryAdjustment = {
  id: string;
  reason: InventoryAdjustmentInput["reason"];
  reference: string | null;
  note: string | null;
  createdAtUtc: string;
  lines: InventoryBalance[];
};

export type InventoryAdjustmentListItem = {
  id: string;
  reason: InventoryAdjustmentInput["reason"];
  reference: string | null;
  createdAtUtc: string;
  lineCount: number;
};

export type InventoryAdjustmentDetail = Omit<InventoryAdjustment, "lines"> & {
  lines: InventoryAdjustmentLine[];
};

export type InventoryAdjustmentLine = {
  movementId: string;
  productId: string;
  productSku: string;
  productName: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  type: "ManualIncrease" | "ManualDecrease";
  unitOfMeasure: string;
  quantityDeltaInUnit: number;
  quantityDelta: number;
  balanceAfter: number;
  createdAtUtc: string;
};

export type InventoryBalance = {
  productId: string;
  warehouseId: string;
  quantity: number;
  updatedAtUtc: string;
  baseUnitOfMeasure: string;
};

export type InventoryMovement = {
  id: string;
  inventoryAdjustmentId: string | null;
  goodsReceiptId: string | null;
  productId: string;
  productSku: string;
  productName: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  adjustmentReference: string | null;
  goodsReceiptNumber: string | null;
  type: "ManualIncrease" | "ManualDecrease" | "GoodsReceipt";
  quantityDelta: number;
  unitOfMeasure: string;
  quantityDeltaInUnit: number;
  balanceAfter: number;
  createdAtUtc: string;
};

export type InventoryMovementFilter = {
  page: number;
  pageSize: number;
  productId?: string;
  warehouseId?: string;
  type?: InventoryMovement["type"];
  reference?: string;
  fromUtc?: string;
  toUtc?: string;
};

export type InventoryAdjustmentListQuery = {
  page: number;
  pageSize: number;
  reason?: InventoryAdjustmentInput["reason"];
  reference?: string;
  fromUtc?: string;
  toUtc?: string;
};

export type PagedInventoryMovements = {
  items: InventoryMovement[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type PagedInventoryAdjustments = {
  items: InventoryAdjustmentListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};
