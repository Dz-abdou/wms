import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createGoodsReceipt,
  getGoodsReceipt,
  getGoodsReceiptCandidate,
  getGoodsReceipts,
} from "./receivingApi";
import type {
  GoodsReceiptInput,
  GoodsReceiptListQuery,
} from "./receivingTypes";

export const receivingKeys = {
  all: ["receiving"] as const,
  list: (query: GoodsReceiptListQuery) =>
    [...receivingKeys.all, "list", query] as const,
  detail: (id: string) => [...receivingKeys.all, "detail", id] as const,
  candidate: (purchaseOrderId: string) =>
    [...receivingKeys.all, "candidate", purchaseOrderId] as const,
};

export function useGoodsReceipts(query: GoodsReceiptListQuery) {
  return useQuery({
    queryKey: receivingKeys.list(query),
    queryFn: ({ signal }) => getGoodsReceipts(query, signal),
  });
}

export function useGoodsReceipt(id: string | undefined) {
  return useQuery({
    queryKey: receivingKeys.detail(id ?? ""),
    queryFn: ({ signal }) => getGoodsReceipt(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useGoodsReceiptCandidate(purchaseOrderId: string | undefined) {
  return useQuery({
    queryKey: receivingKeys.candidate(purchaseOrderId ?? ""),
    queryFn: ({ signal }) =>
      getGoodsReceiptCandidate(purchaseOrderId ?? "", signal),
    enabled: Boolean(purchaseOrderId),
  });
}

export function useCreateGoodsReceipt() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: GoodsReceiptInput) => createGoodsReceipt(input),
    onSuccess: () => {
      return Promise.all([
        queryClient.invalidateQueries({ queryKey: receivingKeys.all }),
        queryClient.invalidateQueries({ queryKey: ["purchasing"] }),
        queryClient.invalidateQueries({ queryKey: ["inventory"] }),
      ]);
    },
  });
}
