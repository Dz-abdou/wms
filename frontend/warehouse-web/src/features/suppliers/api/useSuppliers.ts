import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createSupplier,
  getSupplier,
  getSuppliers,
  setSupplierStatus,
  updateSupplier,
} from "./suppliersApi";
import type { Supplier, SupplierInput } from "./supplierTypes";

export const supplierKeys = {
  all: ["suppliers"] as const,
  list: (page: number, pageSize: number) =>
    [...supplierKeys.all, "list", page, pageSize] as const,
  detail: (id: string) => [...supplierKeys.all, "detail", id] as const,
};

export function useSuppliers(page: number, pageSize: number) {
  return useQuery({
    queryKey: supplierKeys.list(page, pageSize),
    queryFn: ({ signal }) => getSuppliers(page, pageSize, signal),
  });
}

export function useSupplier(id: string | undefined) {
  return useQuery({
    queryKey: supplierKeys.detail(id ?? ""),
    queryFn: ({ signal }) => getSupplier(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useCreateSupplier() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createSupplier,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: supplierKeys.all }),
  });
}

export function useUpdateSupplier(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: SupplierInput) => updateSupplier(id, input),
    onSuccess: (supplier) => refresh(queryClient, supplier),
  });
}

export function useSetSupplierStatus(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (isActive: boolean) => setSupplierStatus(id, isActive),
    onSuccess: (supplier) => refresh(queryClient, supplier),
  });
}

function refresh(queryClient: ReturnType<typeof useQueryClient>, supplier: Supplier) {
  queryClient.setQueryData(supplierKeys.detail(supplier.id), supplier);
  return queryClient.invalidateQueries({ queryKey: supplierKeys.all });
}
