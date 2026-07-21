import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createWarehouse, getWarehouse, getWarehouses, setWarehouseStatus, updateWarehouse } from './warehousesApi'
import type { Warehouse, WarehouseInput } from './warehouseTypes'
export const warehouseKeys = { all: ['warehouses'] as const, list: (page: number, pageSize: number) => [...warehouseKeys.all, 'list', page, pageSize] as const, detail: (id: string) => [...warehouseKeys.all, 'detail', id] as const }
export function useWarehouses(page: number, pageSize: number) { return useQuery({ queryKey: warehouseKeys.list(page, pageSize), queryFn: ({ signal }) => getWarehouses(page, pageSize, signal) }) }
export function useWarehouse(id: string | undefined) { return useQuery({ queryKey: warehouseKeys.detail(id ?? ''), queryFn: ({ signal }) => getWarehouse(id ?? '', signal), enabled: Boolean(id) }) }
export function useCreateWarehouse() { const c = useQueryClient(); return useMutation({ mutationFn: createWarehouse, onSuccess: () => c.invalidateQueries({ queryKey: warehouseKeys.all }) }) }
export function useUpdateWarehouse(id: string) { const c = useQueryClient(); return useMutation({ mutationFn: (input: WarehouseInput) => updateWarehouse(id, input), onSuccess: (w) => refresh(c,w) }) }
export function useSetWarehouseStatus(id: string) { const c = useQueryClient(); return useMutation({ mutationFn: (isActive: boolean) => setWarehouseStatus(id,isActive), onSuccess: (w) => refresh(c,w) }) }
function refresh(c: ReturnType<typeof useQueryClient>, w: Warehouse) { c.setQueryData(warehouseKeys.detail(w.id),w); return c.invalidateQueries({ queryKey: warehouseKeys.all }) }