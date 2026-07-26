import { requestJson } from "../../../shared/api/apiClient";
import { inventoryApiPaths } from "../inventoryConstants";
import type {
  InventoryAdjustment,
  InventoryAdjustmentDetail,
  InventoryAdjustmentInput,
  InventoryMovementFilter,
  PagedInventoryAdjustments,
  PagedInventoryMovements,
} from "./inventoryTypes";

export function adjustInventory(input: InventoryAdjustmentInput) {
  return requestJson<InventoryAdjustment>(inventoryApiPaths.adjustments, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
}

export function getMovementHistory(
  filter: InventoryMovementFilter,
  signal?: AbortSignal,
) {
  const query = new URLSearchParams();
  Object.entries(filter).forEach(([key, value]) => {
    if (value !== undefined && value !== "") query.set(key, String(value));
  });
  return requestJson<PagedInventoryMovements>(
    `${inventoryApiPaths.movements}?${query}`,
    { signal },
  );
}

export function getAdjustments(page: number, pageSize: number, signal?: AbortSignal) {
  return requestJson<PagedInventoryAdjustments>(
    `${inventoryApiPaths.adjustments}?${new URLSearchParams({ page: String(page), pageSize: String(pageSize) })}`,
    { signal },
  );
}

export function getAdjustment(id: string, signal?: AbortSignal) {
  return requestJson<InventoryAdjustmentDetail>(
    `${inventoryApiPaths.adjustments}/${id}`,
    { signal },
  );
}
