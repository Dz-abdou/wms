import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  adjustInventory,
  getAdjustment,
  getAdjustments,
  getInventoryOverview,
  getMovementHistory,
  createCycleCount,
  getCycleCount,
  getCycleCounts,
  createTransfer,
  getTransfer,
  getTransfers,
} from "./inventoryApi";
import type {
  InventoryAdjustmentInput,
  InventoryAdjustmentListQuery,
  InventoryMovementFilter,
  InventoryOverviewQuery,
  CycleCountInput,
  CycleCountListQuery,
  InventoryTransferInput,
  InventoryTransferListQuery,
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
  cycleCounts: (query: CycleCountListQuery) =>
    [...inventoryKeys.all, "cycle-counts", query] as const,
  cycleCount: (id: string) =>
    [...inventoryKeys.all, "cycle-count", id] as const,
  transfers: (query: InventoryTransferListQuery) =>
    [...inventoryKeys.all, "transfers", query] as const,
  transfer: (id: string) => [...inventoryKeys.all, "transfer", id] as const,
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

export function useCycleCounts(query: CycleCountListQuery) {
  return useQuery({
    queryKey: inventoryKeys.cycleCounts(query),
    queryFn: ({ signal }) => getCycleCounts(query, signal),
  });
}

export function useCycleCount(id: string | undefined) {
  return useQuery({
    queryKey: inventoryKeys.cycleCount(id ?? ""),
    queryFn: ({ signal }) => getCycleCount(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useCreateCycleCount() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CycleCountInput) => createCycleCount(input),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: inventoryKeys.all }),
  });
}

export function useTransfers(query: InventoryTransferListQuery) {
  return useQuery({
    queryKey: inventoryKeys.transfers(query),
    queryFn: ({ signal }) => getTransfers(query, signal),
  });
}

export function useTransfer(id: string | undefined) {
  return useQuery({
    queryKey: inventoryKeys.transfer(id ?? ""),
    queryFn: ({ signal }) => getTransfer(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useCreateTransfer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: InventoryTransferInput) => createTransfer(input),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: inventoryKeys.all }),
  });
}
