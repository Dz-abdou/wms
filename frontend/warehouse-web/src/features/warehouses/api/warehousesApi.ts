import { requestJson } from "../../../shared/api/apiClient";
import { warehouseApiPaths } from "../warehouseConstants";
import type {
  Warehouse,
  WarehouseInput,
  WarehouseListQuery,
  WarehouseListResult,
} from "./warehouseTypes";
export function getWarehouses(query: WarehouseListQuery, signal?: AbortSignal) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.search?.trim()) parameters.set("search", query.search.trim());
  if (query.isActive !== undefined)
    parameters.set("isActive", String(query.isActive));
  return requestJson<WarehouseListResult>(
    `${warehouseApiPaths.base}?${parameters}`,
    { signal },
  );
}
export function getWarehouse(id: string, signal?: AbortSignal) {
  return requestJson<Warehouse>(warehouseApiPaths.byId(id), { signal });
}
export function createWarehouse(input: WarehouseInput) {
  return requestJson<Warehouse>(warehouseApiPaths.base, request("POST", input));
}
export function updateWarehouse(id: string, input: WarehouseInput) {
  return requestJson<Warehouse>(
    warehouseApiPaths.byId(id),
    request("PUT", input),
  );
}
export function setWarehouseStatus(id: string, isActive: boolean) {
  return requestJson<Warehouse>(
    warehouseApiPaths.status(id),
    request("PATCH", { isActive }),
  );
}
function request(method: "PATCH" | "POST" | "PUT", body: object): RequestInit {
  return {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  };
}
