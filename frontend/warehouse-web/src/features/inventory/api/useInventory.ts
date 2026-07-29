import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  adjustInventory,
  getAdjustment,
  getAdjustments,
  getInventoryOverview,
  getMovementHistory,
} from "./inventoryApi";
import type {
  InventoryAdjustmentInput,
  InventoryAdjustmentListQuery,
  InventoryMovementFilter,
  InventoryOverviewQuery,
} from "./inventoryTypes";

export const inventoryKeys = {
  all: ["inventory"] as const,
  movements: (filter: InventoryMovementFilter) =>
    [...inventoryKeys.all, "movements", filter] as const,
  overview: (query: InventoryOverviewQuery) =>
    [...inventoryKeys.all, "overview", query] as const,
  adjustments: (query: InventoryAdjustmentListQuery) =>
    [...inventoryKeys.all, "adjustments", query] as const,
  adjustment: (id: string) => [...inventoryKeys.all, "adjustment", id] as const,
};

export function useMovementHistory(filter: InventoryMovementFilter) {
  return useQuery({
    queryKey: inventoryKeys.movements(filter),
    queryFn: ({ signal }) => getMovementHistory(filter, signal),
  });
}

export function useInventoryOverview(query: InventoryOverviewQuery) {
  return useQuery({
    queryKey: inventoryKeys.overview(query),
    queryFn: ({ signal }) => getInventoryOverview(query, signal),
  });
}

export function useInventoryAdjustments(query: InventoryAdjustmentListQuery) {
  return useQuery({
    queryKey: inventoryKeys.adjustments(query),
    queryFn: ({ signal }) => getAdjustments(query, signal),
  });
}

export function useInventoryAdjustment(id: string | undefined) {
  return useQuery({
    queryKey: inventoryKeys.adjustment(id ?? ""),
    queryFn: ({ signal }) => getAdjustment(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useAdjustInventory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: InventoryAdjustmentInput) => adjustInventory(input),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: inventoryKeys.all }),
  });
}
