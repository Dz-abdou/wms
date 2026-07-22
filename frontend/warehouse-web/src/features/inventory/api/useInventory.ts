import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { adjustInventory, getMovementHistory } from './inventoryApi'
import type { InventoryAdjustmentInput } from './inventoryTypes'

export const inventoryKeys = {
  all: ['inventory'] as const,
  movements: (productId: string, warehouseId: string) => [...inventoryKeys.all, 'movements', productId, warehouseId] as const,
}

export function useMovementHistory(productId: string | undefined, warehouseId: string | undefined) {
  return useQuery({
    queryKey: inventoryKeys.movements(productId ?? '', warehouseId ?? ''),
    queryFn: ({ signal }) => getMovementHistory(productId ?? '', warehouseId ?? '', signal),
    enabled: Boolean(productId && warehouseId),
  })
}

export function useAdjustInventory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: InventoryAdjustmentInput) => adjustInventory(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: inventoryKeys.all }),
  })
}
