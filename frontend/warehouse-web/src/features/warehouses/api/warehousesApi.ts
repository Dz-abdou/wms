import { requestJson } from '../../../shared/api/apiClient'
import { warehouseApiPaths } from '../warehouseConstants'
import type { Warehouse, WarehouseInput, WarehouseListResult } from './warehouseTypes'
export function getWarehouses(page: number, pageSize: number, signal?: AbortSignal) { return requestJson<WarehouseListResult>(`${warehouseApiPaths.base}?page=${page}&pageSize=${pageSize}`, { signal }) }
export function getWarehouse(id: string, signal?: AbortSignal) { return requestJson<Warehouse>(warehouseApiPaths.byId(id), { signal }) }
export function createWarehouse(input: WarehouseInput) { return requestJson<Warehouse>(warehouseApiPaths.base, request('POST', input)) }
export function updateWarehouse(id: string, input: WarehouseInput) { return requestJson<Warehouse>(warehouseApiPaths.byId(id), request('PUT', input)) }
export function setWarehouseStatus(id: string, isActive: boolean) { return requestJson<Warehouse>(warehouseApiPaths.status(id), request('PATCH', { isActive })) }
function request(method: 'PATCH' | 'POST' | 'PUT', body: object): RequestInit { return { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) } }