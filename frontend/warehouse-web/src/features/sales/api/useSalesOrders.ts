import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  cancelSalesOrder,
  createSalesOrder,
  getSalesOrder,
  getSalesOrderAvailability,
  getSalesOrders,
  submitSalesOrder,
  updateSalesOrder,
} from "./salesApi";
import type {
  SalesOrder,
  SalesOrderInput,
  SalesOrderListQuery,
} from "./salesTypes";

export const salesOrderKeys = {
  all: ["sales-orders"] as const,
  list: (query: SalesOrderListQuery) =>
    [...salesOrderKeys.all, "list", query] as const,
  detail: (id: string) => [...salesOrderKeys.all, "detail", id] as const,
};
export function useSalesOrders(query: SalesOrderListQuery) {
  return useQuery({
    queryKey: salesOrderKeys.list(query),
    queryFn: ({ signal }) => getSalesOrders(query, signal),
  });
}
export function useSalesOrder(id: string | undefined) {
  return useQuery({
    queryKey: salesOrderKeys.detail(id ?? ""),
    queryFn: ({ signal }) => getSalesOrder(id ?? "", signal),
    enabled: Boolean(id),
  });
}
export function useSalesOrderAvailability(
  fulfillmentWarehouseId: string | undefined,
  productIds: string[],
) {
  const uniqueProductIds = [...new Set(productIds)].sort();
  return useQuery({
    queryKey: [
      ...salesOrderKeys.all,
      "availability",
      fulfillmentWarehouseId,
      uniqueProductIds,
    ],
    queryFn: ({ signal }) =>
      getSalesOrderAvailability(
        fulfillmentWarehouseId ?? "",
        uniqueProductIds,
        signal,
      ),
    enabled: Boolean(fulfillmentWarehouseId) && uniqueProductIds.length > 0,
  });
}
export function useCreateSalesOrder() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createSalesOrder,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: salesOrderKeys.all }),
  });
}
export function useUpdateSalesOrder(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: SalesOrderInput) => updateSalesOrder(id, input),
    onSuccess: (order) => refresh(queryClient, order),
  });
}
export function useSubmitSalesOrder(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (version: number) => submitSalesOrder(id, version),
    onSuccess: (order) => refresh(queryClient, order),
  });
}
export function useCancelSalesOrder(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ version, reason }: { version: number; reason?: string }) =>
      cancelSalesOrder(id, version, reason),
    onSuccess: (order) => refresh(queryClient, order),
  });
}
function refresh(
  queryClient: ReturnType<typeof useQueryClient>,
  order: SalesOrder,
) {
  queryClient.setQueryData(salesOrderKeys.detail(order.id), order);
  return queryClient.invalidateQueries({ queryKey: salesOrderKeys.all });
}
