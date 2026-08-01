import { requestJson } from "../../../shared/api/apiClient";
import { inventoryApiPaths } from "../inventoryConstants";
import type {
  InventoryAdjustment,
  InventoryAdjustmentDetail,
  InventoryAdjustmentInput,
  InventoryAdjustmentListQuery,
  InventoryMovementFilter,
  InventoryOverviewQuery,
  CycleCountCandidate,
  CycleCountInput,
  CycleCountDetail,
  CycleCountListQuery,
  PagedInventoryAdjustments,
  PagedCycleCounts,
  PagedInventoryMovements,
  PagedInventoryOverview,
  InventoryTransferInput,
  InventoryTransferListQuery,
  InventoryTransferDetail,
  PagedInventoryTransfers,
  InventoryTransferCandidate,
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

export function getCycleCounts(
  query: CycleCountListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== "") parameters.set(key, String(value));
  });
  return requestJson<PagedCycleCounts>(
    `${inventoryApiPaths.cycleCounts}?${parameters}`,
    { signal },
  );
}

export function getCycleCountCandidate(
  warehouseId: string,
  productId: string,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({ warehouseId, productId });
  return requestJson<CycleCountCandidate>(
    `${inventoryApiPaths.cycleCountCandidate}?${parameters}`,
    { signal },
  );
}

export function createCycleCount(input: CycleCountInput) {
  return requestJson<CycleCountDetail>(inventoryApiPaths.cycleCounts, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
}

export function getCycleCount(id: string, signal?: AbortSignal) {
  return requestJson<CycleCountDetail>(
    `${inventoryApiPaths.cycleCounts}/${id}`,
    { signal },
  );
}

export function createTransfer(input: InventoryTransferInput) {
  return requestJson<InventoryTransferDetail>(inventoryApiPaths.transfers, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
}

export function getTransferCandidate(
  sourceWarehouseId: string,
  productId: string,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({ sourceWarehouseId, productId });
  return requestJson<InventoryTransferCandidate>(
    `${inventoryApiPaths.transferCandidate}?${parameters}`,
    { signal },
  );
}

export function getTransfers(
  query: InventoryTransferListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== "") parameters.set(key, String(value));
  });
  return requestJson<PagedInventoryTransfers>(
    `${inventoryApiPaths.transfers}?${parameters}`,
    { signal },
  );
}

export function getTransfer(id: string, signal?: AbortSignal) {
  return requestJson<InventoryTransferDetail>(
    `${inventoryApiPaths.transfers}/${id}`,
    { signal },
  );
}
