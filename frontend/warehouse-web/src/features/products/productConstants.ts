export const productApiPaths = {
  base: '/api/products',
  byId: (id: string) => `/api/products/${id}`,
  status: (id: string) => `/api/products/${id}/status`
} as const

export const productRoutes = {
  create: '/products/new',
  detail: (id: string) => `/products/${id}`,
  detailPattern: 'products/:id',
  edit: (id: string) => `/products/${id}/edit`,
  editPattern: 'products/:id/edit',
  list: '/products',
  listPattern: 'products'
} as const

export const productPagination = {
  defaultPage: 1,
  defaultPageSize: 20
} as const

export const productValidation = {
  maxDescriptionLength: 1000,
  maxNameLength: 200,
  maxSkuLength: 64
} as const