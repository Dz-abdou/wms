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

export type InventoryBalance = {
  productId: string;
  warehouseId: string;
  quantity: number;
  updatedAtUtc: string;
  baseUnitOfMeasure: string;
};

export type InventoryMovement = {
  id: string;
  productId: string;
  warehouseId: string;
  type: "ManualIncrease" | "ManualDecrease";
  quantityDelta: number;
  unitOfMeasure: string;
  quantityDeltaInUnit: number;
  balanceAfter: number;
  createdAtUtc: string;
};

export type PagedInventoryMovements = {
  items: InventoryMovement[];
  page: number;
  pageSize: number;
  totalCount: number;
};
