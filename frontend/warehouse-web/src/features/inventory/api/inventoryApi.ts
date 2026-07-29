import { requestJson } from "../../../shared/api/apiClient";
import { inventoryApiPaths } from "../inventoryConstants";
import type {
  InventoryAdjustment,
  InventoryAdjustmentDetail,
  InventoryAdjustmentInput,
  InventoryAdjustmentListQuery,
  InventoryMovementFilter,
  InventoryOverviewQuery,
  PagedInventoryAdjustments,
  PagedInventoryMovements,
  PagedInventoryOverview,
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

export function getInventoryOverview(
  query: InventoryOverviewQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== "") parameters.set(key, String(value));
  });
  return requestJson<PagedInventoryOverview>(
    `${inventoryApiPaths.overview}?${parameters}`,
    { signal },
  );
}

export function getAdjustments(
  query: InventoryAdjustmentListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== "") parameters.set(key, String(value));
  });
  return requestJson<PagedInventoryAdjustments>(
    `${inventoryApiPaths.adjustments}?${parameters}`,
    { signal },
  );
}

export function getAdjustment(id: string, signal?: AbortSignal) {
  return requestJson<InventoryAdjustmentDetail>(
    `${inventoryApiPaths.adjustments}/${id}`,
    { signal },
  );
}
