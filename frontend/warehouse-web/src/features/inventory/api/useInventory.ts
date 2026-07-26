import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { adjustInventory, getAdjustment, getAdjustments, getMovementHistory } from './inventoryApi'
import type { InventoryAdjustmentInput, InventoryMovementFilter } from './inventoryTypes'

export const inventoryKeys = {
  all: ['inventory'] as const,
  movements: (filter: InventoryMovementFilter) => [...inventoryKeys.all, 'movements', filter] as const,
  adjustments: (page: number, pageSize: number) => [...inventoryKeys.all, 'adjustments', page, pageSize] as const,
  adjustment: (id: string) => [...inventoryKeys.all, 'adjustment', id] as const,
}

export function useMovementHistory(filter: InventoryMovementFilter) {
  return useQuery({
    queryKey: inventoryKeys.movements(filter),
    queryFn: ({ signal }) => getMovementHistory(filter, signal),
  })
}

export function useInventoryAdjustments(page: number, pageSize: number) {
  return useQuery({ queryKey: inventoryKeys.adjustments(page, pageSize), queryFn: ({ signal }) => getAdjustments(page, pageSize, signal) })
}

export function useInventoryAdjustment(id: string | undefined) {
  return useQuery({ queryKey: inventoryKeys.adjustment(id ?? ''), queryFn: ({ signal }) => getAdjustment(id ?? '', signal), enabled: Boolean(id) })
}

export function useAdjustInventory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: InventoryAdjustmentInput) => adjustInventory(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: inventoryKeys.all }),
  })
}
