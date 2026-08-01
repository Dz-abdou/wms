export const salesApiPaths = {
  orders: "/api/sales-orders",
  orderById: (id: string) => `/api/sales-orders/${id}`,
  submit: (id: string) => `/api/sales-orders/${id}/submit`,
  cancel: (id: string) => `/api/sales-orders/${id}/cancel`,
} as const;

export const salesRoutes = {
  orders: "/sales-orders",
  ordersPattern: "sales-orders",
  create: "/sales-orders/new",
  createPattern: "sales-orders/new",
  detail: (id: string) => `/sales-orders/${id}`,
  detailPattern: "sales-orders/:id",
  edit: (id: string) => `/sales-orders/${id}/edit`,
  editPattern: "sales-orders/:id/edit",
} as const;
