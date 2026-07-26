import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createProductCategory,
  getProductCategories,
  getProductCategory,
  updateProductCategory,
} from "./productCategoriesApi";
import type { ProductCategory, ProductCategoryListQuery } from "./productTypes";

export const productCategoryKeys = {
  all: ["product-categories"] as const,
  list: (query: ProductCategoryListQuery) =>
    [...productCategoryKeys.all, "list", query] as const,
  detail: (id: string) => [...productCategoryKeys.all, "detail", id] as const,
};

export function useProductCategories(query: ProductCategoryListQuery) {
  return useQuery({
    queryKey: productCategoryKeys.list(query),
    queryFn: ({ signal }) => getProductCategories(query, signal),
  });
}

export function useProductCategory(id: string | undefined) {
  return useQuery({
    queryKey: productCategoryKeys.detail(id ?? ""),
    queryFn: ({ signal }) => getProductCategory(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useCreateProductCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createProductCategory,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: productCategoryKeys.all }),
  });
}
export function useUpdateProductCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      input,
    }: {
      id: string;
      input: Pick<ProductCategory, "code" | "name" | "parentCategoryId">;
    }) => updateProductCategory(id, input),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: productCategoryKeys.all }),
  });
}
