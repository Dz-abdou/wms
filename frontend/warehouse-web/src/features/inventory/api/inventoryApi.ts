import { requestJson } from "../../../shared/api/apiClient";
import { inventoryApiPaths } from "../inventoryConstants";
import type {
  InventoryAdjustment,
  InventoryAdjustmentInput,
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
  productId: string,
  warehouseId: string,
  signal?: AbortSignal,
) {
  const query = new URLSearchParams({ productId, warehouseId });
  return requestJson<PagedInventoryMovements>(
    `${inventoryApiPaths.movements}?${query}`,
    { signal },
  );
}
