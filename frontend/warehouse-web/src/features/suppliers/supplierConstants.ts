export const supplierApiPaths = {
  base: "/api/suppliers",
  byId: (id: string) => `/api/suppliers/${id}`,
  status: (id: string) => `/api/suppliers/${id}/status`,
} as const;

export const supplierRoutes = {
  list: "/suppliers",
  listPattern: "suppliers",
  create: "/suppliers/new",
  detail: (id: string) => `/suppliers/${id}`,
  detailPattern: "suppliers/:id",
  edit: (id: string) => `/suppliers/${id}/edit`,
  editPattern: "suppliers/:id/edit",
} as const;

export const supplierValidation = {
  maxCodeLength: 32,
  maxNameLength: 200,
  maxEmailLength: 320,
  maxPhoneNumberLength: 50,
  maxAddressLength: 500,
} as const;
