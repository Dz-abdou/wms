export type SupplierProduct = {
  id: string;
  supplierId: string;
  supplierCode: string;
  supplierName: string;
  productId: string;
  productSku: string;
  productName: string;
  supplierSku: string | null;
  purchaseUnitOfMeasure: string;
  minimumOrderQuantity: number;
  unitPrice: number;
  currencyCode: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type Currency = {
  id: string;
  code: string;
  name: string;
  symbol: string | null;
  decimalPlaces: number;
  isActive: boolean;
  isDefault: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type CurrencyInput = Pick<
  Currency,
  "code" | "name" | "symbol" | "decimalPlaces"
>;

export type CurrencyListQuery = {
  page: number;
  pageSize: number;
  search?: string;
  isActive?: boolean;
};

export type SupplierProductInput = {
  supplierId: string;
  productId: string;
  supplierSku?: string;
  purchaseUnitOfMeasure: string;
  minimumOrderQuantity: number;
  unitPrice: number;
  currencyCode: string;
};

export type UpdateSupplierProductInput = Omit<
  SupplierProductInput,
  "supplierId" | "productId"
>;

export type SupplierProductListResult = {
  items: SupplierProduct[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type SupplierProductListQuery = {
  page: number;
  pageSize: number;
  supplierId?: string;
  productId?: string;
  isActive?: boolean;
  currencyCode?: string;
};

export type PurchaseOrderStatus = 0 | 1 | 2 | 3 | 4;

export const purchaseOrderStatusTranslationKeys: Record<
  PurchaseOrderStatus,
  string
> = {
  0: "purchasing.status.draft",
  1: "purchasing.status.submitted",
  2: "purchasing.status.partiallyReceived",
  3: "purchasing.status.received",
  4: "purchasing.status.cancelled",
};

export const purchaseOrderStatusColors: Record<PurchaseOrderStatus, string> = {
  0: "gold",
  1: "blue",
  2: "cyan",
  3: "green",
  4: "default",
};

export type PurchaseOrderLineInput = {
  supplierProductId: string;
  quantity: number;
};

export type PurchaseOrderInput = {
  supplierId: string;
  destinationWarehouseId?: string;
  currencyCode?: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  supplierReference?: string;
  notes?: string;
  version?: number;
  lines: PurchaseOrderLineInput[];
};

export type PurchaseOrderLine = PurchaseOrderLineInput & {
  id: string;
  lineNumber: number;
  productId: string;
  productSku: string;
  productName: string;
  supplierSku: string | null;
  purchaseUnitOfMeasure: string;
  quantityInBaseUnit: number;
  conversionFactorToBaseUnit: number;
  unitPrice: number;
  currencyCode: string;
  lineAmount: number;
};

export type PurchaseOrder = {
  id: string;
  supplierId: string;
  supplierCode: string;
  supplierName: string;
  number?: string;
  destinationWarehouseId?: string;
  destinationWarehouseCode?: string;
  destinationWarehouseName?: string;
  currencyCode?: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  buyerUserId?: string;
  supplierReference?: string;
  notes?: string;
  status: PurchaseOrderStatus;
  lines: PurchaseOrderLine[];
  totalAmount: number;
  version: number;
  submittedAtUtc?: string;
  statusHistory: PurchaseOrderStatusHistory[];
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type PurchaseOrderListResult = {
  items: PurchaseOrder[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type PurchaseOrderListQuery = {
  page: number;
  pageSize: number;
  supplierId?: string;
  status?: PurchaseOrderStatus;
  warehouseId?: string;
  fromOrderDate?: string;
  toOrderDate?: string;
};

export type PurchaseOrderStatusHistory = {
  id: string;
  previousStatus: PurchaseOrderStatus | null;
  status: PurchaseOrderStatus;
  changedAtUtc: string;
  actorUserId: string;
  reason: string | null;
};
