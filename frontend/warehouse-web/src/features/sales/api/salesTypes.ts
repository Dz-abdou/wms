export type SalesOrderStatus = "Draft" | "Submitted" | "Cancelled";

export const salesOrderStatusTranslationKeys: Record<SalesOrderStatus, string> =
  {
    Draft: "sales.status.draft",
    Submitted: "sales.status.submitted",
    Cancelled: "sales.status.cancelled",
  };

export const salesOrderStatusColors: Record<SalesOrderStatus, string> = {
  Draft: "gold",
  Submitted: "blue",
  Cancelled: "default",
};

export type SalesOrderLineInput = {
  productId: string;
  unitOfMeasure?: string;
  quantity: number;
};

export type SalesOrderInput = {
  customerId: string;
  shippingAddressId: string;
  currencyCode: string;
  orderDate: string;
  requestedShipDate?: string;
  customerReference?: string;
  deliveryInstructions?: string;
  version?: number;
  lines: SalesOrderLineInput[];
};

export type SalesOrderLine = SalesOrderLineInput & {
  id: string;
  lineNumber: number;
  productSku: string;
  productName: string;
  quantityInBaseUnit: number;
  conversionFactorToBaseUnit: number;
};

export type SalesOrderAddress = {
  label: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  postalCode: string | null;
  countryCode: string;
  deliveryInstructions: string | null;
};

export type SalesOrderStatusHistory = {
  id: string;
  previousStatus: SalesOrderStatus | null;
  status: SalesOrderStatus;
  changedAtUtc: string;
  actorUserId: string;
  reason: string | null;
};

export type SalesOrder = {
  id: string;
  number: string;
  customerId: string;
  customerCode: string;
  customerName: string;
  shippingAddressId: string;
  shippingAddress: SalesOrderAddress;
  currencyCode: string;
  orderDate: string;
  requestedShipDate: string | null;
  customerReference: string | null;
  deliveryInstructions: string | null;
  ownerUserId: string;
  status: SalesOrderStatus;
  lines: SalesOrderLine[];
  version: number;
  submittedAtUtc: string | null;
  statusHistory: SalesOrderStatusHistory[];
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type SalesOrderListQuery = {
  page: number;
  pageSize: number;
  status?: SalesOrderStatus;
  customerId?: string;
  fromOrderDate?: string;
  toOrderDate?: string;
};

export type SalesOrderListResult = {
  items: SalesOrder[];
  page: number;
  pageSize: number;
  totalCount: number;
};
