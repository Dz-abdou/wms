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

export type InventoryOverviewItem = {
  productId: string;
  productSku: string;
  productName: string;
  productIsActive: boolean;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  quantity: number;
  baseUnitOfMeasure: string;
  updatedAtUtc: string;
};

export type InventoryMovement = {
  id: string;
  inventoryAdjustmentId: string | null;
  goodsReceiptId: string | null;
  cycleCountId: string | null;
  productId: string;
  productSku: string;
  productName: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  adjustmentReference: string | null;
  goodsReceiptNumber: string | null;
  cycleCountReference: string | null;
  type:
    | "ManualIncrease"
    | "ManualDecrease"
    | "GoodsReceipt"
    | "CycleCountIncrease"
    | "CycleCountDecrease";
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

export type InventoryOverviewQuery = {
  page: number;
  pageSize: number;
  search?: string;
  warehouseId?: string;
  categoryId?: string;
  isActive?: boolean;
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

export type PagedInventoryOverview = {
  items: InventoryOverviewItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type CycleCountLineInput = {
  productId: string;
  systemQuantityInBase: number;
  systemBalanceVersion: number;
  countedUnitOfMeasure: string;
  countedQuantityInUnit: number;
};

export type CycleCountInput = {
  warehouseId: string;
  reference?: string;
  note?: string;
  lines: CycleCountLineInput[];
};

export type CycleCountCandidate = {
  productId: string;
  productSku: string;
  productName: string;
  baseUnitOfMeasure: string;
  systemQuantityInBase: number;
  systemBalanceVersion: number;
};

export type CycleCountListQuery = {
  page: number;
  pageSize: number;
  warehouseId?: string;
  reference?: string;
  fromUtc?: string;
  toUtc?: string;
};

export type CycleCountListItem = {
  id: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  reference: string | null;
  countedAtUtc: string;
  lineCount: number;
  varianceLineCount: number;
};

export type CycleCountLine = CycleCountLineInput & {
  id: string;
  lineNumber: number;
  productSku: string;
  productName: string;
  baseUnitOfMeasure: string;
  countedQuantityInBase: number;
  varianceQuantityInBase: number;
  inventoryMovementId: string | null;
};

export type CycleCountDetail = Omit<CycleCountInput, "lines"> & {
  id: string;
  warehouseCode: string;
  warehouseName: string;
  countedAtUtc: string;
  lines: CycleCountLine[];
};

export type PagedCycleCounts = {
  items: CycleCountListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};
