export type InventoryAdjustmentDirection = 'Increase' | 'Decrease'

export type InventoryAdjustmentInput = {
  productId: string
  warehouseId: string
  quantity: number
  direction: InventoryAdjustmentDirection
}

export type InventoryBalance = {
  productId: string
  warehouseId: string
  quantity: number
  updatedAtUtc: string
}

export type InventoryMovement = {
  id: string
  productId: string
  warehouseId: string
  type: 'ManualIncrease' | 'ManualDecrease'
  quantityDelta: number
  balanceAfter: number
  createdAtUtc: string
}

export type PagedInventoryMovements = {
  items: InventoryMovement[]
  page: number
  pageSize: number
  totalCount: number
}
