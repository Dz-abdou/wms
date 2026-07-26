import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createPurchaseOrder,
  createCurrency,
  getCurrency,
  getCurrencies,
  createSupplierProduct,
  getPurchasingCurrencies,
  getPurchaseOrder,
  getPurchaseOrders,
  getSupplierProduct,
  getSupplierProducts,
  setSupplierProductStatus,
  setCurrencyStatus,
  setDefaultCurrency,
  updateCurrency,
  submitPurchaseOrder,
  updatePurchaseOrder,
  updateSupplierProduct,
} from "./purchasingApi";
import type {
  CurrencyListQuery,
  PurchaseOrder,
  PurchaseOrderInput,
  PurchaseOrderListQuery,
  SupplierProduct,
  SupplierProductListQuery,
  UpdateSupplierProductInput,
} from "./purchasingTypes";

export const purchasingKeys = {
  all: ["purchasing"] as const,
  currencies: (query?: CurrencyListQuery) =>
    [...purchasingKeys.all, "currencies", query ?? "selector"] as const,
  currencyDetail: (id: string) =>
    [...purchasingKeys.all, "currency", id] as const,
  catalogue: (query: SupplierProductListQuery) =>
    [...purchasingKeys.all, "catalogue", query] as const,
  catalogueDetail: (id: string) =>
    [...purchasingKeys.all, "catalogue", id] as const,
  orders: (query: PurchaseOrderListQuery) =>
    [...purchasingKeys.all, "orders", query] as const,
  orderDetail: (id: string) => [...purchasingKeys.all, "order", id] as const,
};

export function usePurchasingCurrencies() {
  return useQuery({
    queryKey: purchasingKeys.currencies(),
    queryFn: ({ signal }) => getPurchasingCurrencies(signal),
    staleTime: Infinity,
  });
}

export function useCurrencies(query: CurrencyListQuery) {
  return useQuery({
    queryKey: purchasingKeys.currencies(query),
    queryFn: ({ signal }) => getCurrencies(query, signal),
  });
}
export function useCurrency(id: string | undefined) {
  return useQuery({
    queryKey: purchasingKeys.currencyDetail(id ?? ""),
    queryFn: ({ signal }) => getCurrency(id ?? "", signal),
    enabled: Boolean(id),
  });
}
export function useCreateCurrency() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createCurrency,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: purchasingKeys.all }),
  });
}
export function useUpdateCurrency() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      input,
    }: {
      id: string;
      input: Omit<import("./purchasingTypes").CurrencyInput, "code">;
    }) => updateCurrency(id, input),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: purchasingKeys.all }),
  });
}
export function useSetCurrencyStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      setCurrencyStatus(id, isActive),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: purchasingKeys.all }),
  });
}
export function useSetDefaultCurrency() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: setDefaultCurrency,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: purchasingKeys.all }),
  });
}

export function useSupplierProducts(query: SupplierProductListQuery) {
  return useQuery({
    queryKey: purchasingKeys.catalogue(query),
    queryFn: ({ signal }) => getSupplierProducts(query, signal),
  });
}

export function useSupplierProduct(id: string | undefined) {
  return useQuery({
    queryKey: purchasingKeys.catalogueDetail(id ?? ""),
    queryFn: ({ signal }) => getSupplierProduct(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useCreateSupplierProduct() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createSupplierProduct,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: purchasingKeys.all }),
  });
}

export function useUpdateSupplierProduct(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: UpdateSupplierProductInput) =>
      updateSupplierProduct(id, input),
    onSuccess: (item) => refreshCatalogue(queryClient, item),
  });
}

export function useSetSupplierProductStatus(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (isActive: boolean) => setSupplierProductStatus(id, isActive),
    onSuccess: (item) => refreshCatalogue(queryClient, item),
  });
}

export function usePurchaseOrders(query: PurchaseOrderListQuery) {
  return useQuery({
    queryKey: purchasingKeys.orders(query),
    queryFn: ({ signal }) => getPurchaseOrders(query, signal),
  });
}

export function usePurchaseOrder(id: string | undefined) {
  return useQuery({
    queryKey: purchasingKeys.orderDetail(id ?? ""),
    queryFn: ({ signal }) => getPurchaseOrder(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useCreatePurchaseOrder() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createPurchaseOrder,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: purchasingKeys.all }),
  });
}

export function useUpdatePurchaseOrder(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: PurchaseOrderInput) => updatePurchaseOrder(id, input),
    onSuccess: (order) => refreshOrder(queryClient, order),
  });
}

export function useSubmitPurchaseOrder(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (version: number) => submitPurchaseOrder(id, version),
    onSuccess: (order) => refreshOrder(queryClient, order),
  });
}

function refreshCatalogue(
  queryClient: ReturnType<typeof useQueryClient>,
  item: SupplierProduct,
) {
  queryClient.setQueryData(purchasingKeys.catalogueDetail(item.id), item);
  return queryClient.invalidateQueries({ queryKey: purchasingKeys.all });
}

function refreshOrder(
  queryClient: ReturnType<typeof useQueryClient>,
  order: PurchaseOrder,
) {
  queryClient.setQueryData(purchasingKeys.orderDetail(order.id), order);
  return queryClient.invalidateQueries({ queryKey: purchasingKeys.all });
}
