export const inventoryApiPaths = {
  adjustments: '/api/inventory/adjustments',
  movements: '/api/inventory/movements',
} as const

export const inventoryRoutes = {
  dashboard: '/inventory',
  dashboardPattern: 'inventory',
} as const

export const inventoryPageSize = 100

export const fractionalBaseUnitCodes: ReadonlySet<string> = new Set(['KG', 'G', 'L', 'ML', 'M', 'CM', 'MM'])
