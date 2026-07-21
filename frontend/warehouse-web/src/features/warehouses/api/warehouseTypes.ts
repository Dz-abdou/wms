export type Warehouse = { id: string; code: string; name: string; description: string | null; isActive: boolean; createdAtUtc: string; updatedAtUtc: string }
export type WarehouseInput = { code: string; name: string; description?: string }
export type WarehouseListResult = { items: Warehouse[]; page: number; pageSize: number; totalCount: number }