export const purchasingApiPaths = {
  currencies: "/api/purchasing/currencies",
  supplierProducts: "/api/supplier-products",
  supplierProductById: (id: string) => `/api/supplier-products/${id}`,
  supplierProductStatus: (id: string) => `/api/supplier-products/${id}/status`,
  purchaseOrders: "/api/purchase-orders",
  purchaseOrderById: (id: string) => `/api/purchase-orders/${id}`,
  purchaseOrderSubmit: (id: string) => `/api/purchase-orders/${id}/submit`,
} as const;

export const purchasingRoutes = {
  catalogue: "/supplier-catalogue",
  cataloguePattern: "supplier-catalogue",
  catalogueCreate: "/supplier-catalogue/new",
  catalogueCreatePattern: "supplier-catalogue/new",
  catalogueEdit: (id: string) => `/supplier-catalogue/${id}/edit`,
  catalogueEditPattern: "supplier-catalogue/:id/edit",
  orders: "/purchase-orders",
  ordersPattern: "purchase-orders",
  orderCreate: "/purchase-orders/new",
  orderDetail: (id: string) => `/purchase-orders/${id}`,
  orderDetailPattern: "purchase-orders/:id",
  orderEdit: (id: string) => `/purchase-orders/${id}/edit`,
  orderEditPattern: "purchase-orders/:id/edit",
} as const;

export const purchasingValidation = {
  maxSupplierSkuLength: 64,
  maxUnitOfMeasureLength: 16,
  currencyCodeLength: 3,
} as const;
