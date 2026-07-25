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

export type CurrencyOption = {
  code: string;
  isDefault: boolean;
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

export type UpdateSupplierProductInput = Omit<SupplierProductInput, "supplierId" | "productId">;

export type SupplierProductListResult = {
  items: SupplierProduct[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type PurchaseOrderStatus = 0 | 1;

export type PurchaseOrderLineInput = {
  supplierProductId: string;
  quantity: number;
};

export type PurchaseOrderInput = {
  supplierId: string;
  lines: PurchaseOrderLineInput[];
};

export type PurchaseOrderLine = PurchaseOrderLineInput & {
  id: string;
  productId: string;
  productSku: string;
  productName: string;
  supplierSku: string | null;
  purchaseUnitOfMeasure: string;
  unitPrice: number;
  currencyCode: string;
};

export type PurchaseOrder = {
  id: string;
  supplierId: string;
  supplierCode: string;
  supplierName: string;
  status: PurchaseOrderStatus;
  lines: PurchaseOrderLine[];
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type PurchaseOrderListResult = {
  items: PurchaseOrder[];
  page: number;
  pageSize: number;
  totalCount: number;
};
