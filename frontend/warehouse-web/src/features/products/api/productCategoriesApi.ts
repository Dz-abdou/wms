import { requestJson } from "../../../shared/api/apiClient";
import type {
  PagedResult,
  ProductCategory,
  ProductCategoryListQuery,
} from "./productTypes";

const productCategoriesPath = "/api/product-categories";

export function getProductCategories(
  query: ProductCategoryListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.search?.trim()) parameters.set("search", query.search.trim());
  return requestJson<PagedResult<ProductCategory>>(
    `${productCategoriesPath}?${parameters}`,
    { signal },
  );
}

export function getProductCategory(id: string, signal?: AbortSignal) {
  return requestJson<ProductCategory>(`${productCategoriesPath}/${id}`, {
    signal,
  });
}

export function createProductCategory(
  input: Pick<ProductCategory, "code" | "name" | "parentCategoryId">,
) {
  return requestJson<ProductCategory>(productCategoriesPath, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
}

export function updateProductCategory(
  id: string,
  input: Pick<ProductCategory, "code" | "name" | "parentCategoryId">,
) {
  return requestJson<ProductCategory>(`${productCategoriesPath}/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
}
