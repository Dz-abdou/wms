export const inventoryApiPaths = {
  adjustments: '/api/inventory/adjustments',
  movements: '/api/inventory/movements',
} as const

export const inventoryRoutes = {
  root: "/inventory",
  rootPattern: "inventory",
  movementHistory: "/inventory/movements",
  movementHistoryPattern: "inventory/movements",
  adjustments: "/inventory/adjustments",
  adjustmentsPattern: "inventory/adjustments",
  adjustmentCreate: "/inventory/adjustments/new",
  adjustmentCreatePattern: "inventory/adjustments/new",
  adjustmentDetail: (id: string) => `/inventory/adjustments/${id}`,
  adjustmentDetailPattern: "inventory/adjustments/:id",
} as const

export const inventoryPageSize = 100

export const fractionalBaseUnitCodes: ReadonlySet<string> = new Set(['KG', 'G', 'L', 'ML', 'M', 'CM', 'MM'])
