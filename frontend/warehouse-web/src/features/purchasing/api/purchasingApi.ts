import { requestJson } from "../../../shared/api/apiClient";
import { purchasingApiPaths } from "../purchasingConstants";
import type {
  Currency,
  CurrencyInput,
  CurrencyListQuery,
  PurchaseOrder,
  PurchaseOrderInput,
  PurchaseOrderListResult,
  SupplierProduct,
  SupplierProductInput,
  SupplierProductListResult,
  SupplierProductListQuery,
  PurchaseOrderListQuery,
  UpdateSupplierProductInput,
} from "./purchasingTypes";

export function getPurchasingCurrencies(signal?: AbortSignal) {
  return requestJson<{ items: Currency[] }>(
    `${purchasingApiPaths.currencies}?activeOnly=true&page=1&pageSize=100`,
    { signal },
  ).then((result) => result.items);
}

export function getCurrencies(query: CurrencyListQuery, signal?: AbortSignal) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.search?.trim()) parameters.set("search", query.search.trim());
  if (query.isActive !== undefined)
    parameters.set("isActive", String(query.isActive));
  return requestJson<{
    items: Currency[];
    page: number;
    pageSize: number;
    totalCount: number;
  }>(`${purchasingApiPaths.currencies}?${parameters}`, { signal });
}
export function getCurrency(id: string, signal?: AbortSignal) {
  return requestJson<Currency>(`${purchasingApiPaths.currencies}/${id}`, {
    signal,
  });
}
export function createCurrency(input: CurrencyInput) {
  return requestJson<Currency>(
    purchasingApiPaths.currencies,
    jsonRequest("POST", input),
  );
}
export function updateCurrency(id: string, input: Omit<CurrencyInput, "code">) {
  return requestJson<Currency>(
    `${purchasingApiPaths.currencies}/${id}`,
    jsonRequest("PUT", input),
  );
}
export function setCurrencyStatus(id: string, isActive: boolean) {
  return requestJson<Currency>(
    `${purchasingApiPaths.currencies}/${id}/status`,
    jsonRequest("PATCH", { isActive }),
  );
}
export function setDefaultCurrency(id: string) {
  return requestJson<Currency>(
    `${purchasingApiPaths.currencies}/${id}/default`,
    jsonRequest("PATCH", {}),
  );
}

export function getSupplierProducts(
  query: SupplierProductListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.supplierId) parameters.set("supplierId", query.supplierId);
  if (query.productId) parameters.set("productId", query.productId);
  if (query.isActive !== undefined)
    parameters.set("isActive", String(query.isActive));
  if (query.currencyCode) parameters.set("currencyCode", query.currencyCode);
  return requestJson<SupplierProductListResult>(
    `${purchasingApiPaths.supplierProducts}?${parameters}`,
    { signal },
  );
}

export function getSupplierProduct(id: string, signal?: AbortSignal) {
  return requestJson<SupplierProduct>(
    purchasingApiPaths.supplierProductById(id),
    { signal },
  );
}

export function createSupplierProduct(input: SupplierProductInput) {
  return requestJson<SupplierProduct>(
    purchasingApiPaths.supplierProducts,
    jsonRequest("POST", input),
  );
}

export function updateSupplierProduct(
  id: string,
  input: UpdateSupplierProductInput,
) {
  return requestJson<SupplierProduct>(
    purchasingApiPaths.supplierProductById(id),
    jsonRequest("PUT", input),
  );
}

export function setSupplierProductStatus(id: string, isActive: boolean) {
  return requestJson<SupplierProduct>(
    purchasingApiPaths.supplierProductStatus(id),
    jsonRequest("PATCH", { isActive }),
  );
}

export function getPurchaseOrders(
  query: PurchaseOrderListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.supplierId) parameters.set("supplierId", query.supplierId);
  if (query.status !== undefined)
    parameters.set("status", String(query.status));
  if (query.warehouseId) parameters.set("warehouseId", query.warehouseId);
  if (query.fromOrderDate) parameters.set("fromOrderDate", query.fromOrderDate);
  if (query.toOrderDate) parameters.set("toOrderDate", query.toOrderDate);
  return requestJson<PurchaseOrderListResult>(
    `${purchasingApiPaths.purchaseOrders}?${parameters}`,
    { signal },
  );
}

export function getPurchaseOrder(id: string, signal?: AbortSignal) {
  return requestJson<PurchaseOrder>(purchasingApiPaths.purchaseOrderById(id), {
    signal,
  });
}

export function createPurchaseOrder(input: PurchaseOrderInput) {
  return requestJson<PurchaseOrder>(
    purchasingApiPaths.purchaseOrders,
    jsonRequest("POST", input),
  );
}

export function updatePurchaseOrder(id: string, input: PurchaseOrderInput) {
  return requestJson<PurchaseOrder>(
    purchasingApiPaths.purchaseOrderById(id),
    jsonRequest("PUT", input),
  );
}

export function submitPurchaseOrder(id: string, version: number) {
  return requestJson<PurchaseOrder>(
    purchasingApiPaths.purchaseOrderSubmit(id),
    jsonRequest("PATCH", { version }),
  );
}

function jsonRequest(
  method: "PATCH" | "POST" | "PUT",
  body: object,
): RequestInit {
  return {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  };
}
