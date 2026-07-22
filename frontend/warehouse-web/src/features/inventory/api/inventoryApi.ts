import { requestJson } from '../../../shared/api/apiClient'
import { inventoryApiPaths } from '../inventoryConstants'
import type { InventoryAdjustmentInput, InventoryBalance, PagedInventoryMovements } from './inventoryTypes'

export function adjustInventory(input: InventoryAdjustmentInput) {
  return requestJson<InventoryBalance>(inventoryApiPaths.adjustments, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  })
}

export function getMovementHistory(productId: string, warehouseId: string, signal?: AbortSignal) {
  const query = new URLSearchParams({ productId, warehouseId })
  return requestJson<PagedInventoryMovements>(`${inventoryApiPaths.movements}?${query}`, { signal })
}
