import { requestJson } from "../../../shared/api/apiClient";
import { purchasingApiPaths } from "../purchasingConstants";
import type {
  Currency,
  CurrencyInput,
  PurchaseOrder,
  PurchaseOrderInput,
  PurchaseOrderListResult,
  SupplierProduct,
  SupplierProductInput,
  SupplierProductListResult,
  UpdateSupplierProductInput,
} from "./purchasingTypes";

export function getPurchasingCurrencies(signal?: AbortSignal) {
  return requestJson<{ items: Currency[] }>(`${purchasingApiPaths.currencies}?activeOnly=true&page=1&pageSize=100`, { signal }).then((result) => result.items);
}

export function getCurrencies(signal?: AbortSignal) { return requestJson<{ items: Currency[] }>(`${purchasingApiPaths.currencies}?page=1&pageSize=100`, { signal }); }
export function createCurrency(input: CurrencyInput) { return requestJson<Currency>(purchasingApiPaths.currencies, jsonRequest("POST", input)); }
export function setCurrencyStatus(id: string, isActive: boolean) { return requestJson<Currency>(`${purchasingApiPaths.currencies}/${id}/status`, jsonRequest("PATCH", { isActive })); }
export function setDefaultCurrency(id: string) { return requestJson<Currency>(`${purchasingApiPaths.currencies}/${id}/default`, jsonRequest("PATCH", {})); }

export function getSupplierProducts(page: number, pageSize: number, supplierId?: string, signal?: AbortSignal) {
  const parameters = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (supplierId) parameters.set("supplierId", supplierId);
  return requestJson<SupplierProductListResult>(`${purchasingApiPaths.supplierProducts}?${parameters}`, { signal });
}

export function getSupplierProduct(id: string, signal?: AbortSignal) {
  return requestJson<SupplierProduct>(purchasingApiPaths.supplierProductById(id), { signal });
}

export function createSupplierProduct(input: SupplierProductInput) {
  return requestJson<SupplierProduct>(purchasingApiPaths.supplierProducts, jsonRequest("POST", input));
}

export function updateSupplierProduct(id: string, input: UpdateSupplierProductInput) {
  return requestJson<SupplierProduct>(purchasingApiPaths.supplierProductById(id), jsonRequest("PUT", input));
}

export function setSupplierProductStatus(id: string, isActive: boolean) {
  return requestJson<SupplierProduct>(purchasingApiPaths.supplierProductStatus(id), jsonRequest("PATCH", { isActive }));
}

export function getPurchaseOrders(page: number, pageSize: number, signal?: AbortSignal) {
  return requestJson<PurchaseOrderListResult>(`${purchasingApiPaths.purchaseOrders}?page=${page}&pageSize=${pageSize}`, { signal });
}

export function getPurchaseOrder(id: string, signal?: AbortSignal) {
  return requestJson<PurchaseOrder>(purchasingApiPaths.purchaseOrderById(id), { signal });
}

export function createPurchaseOrder(input: PurchaseOrderInput) {
  return requestJson<PurchaseOrder>(purchasingApiPaths.purchaseOrders, jsonRequest("POST", input));
}

export function updatePurchaseOrder(id: string, input: PurchaseOrderInput) {
  return requestJson<PurchaseOrder>(purchasingApiPaths.purchaseOrderById(id), jsonRequest("PUT", input));
}

export function submitPurchaseOrder(id: string) {
  return requestJson<PurchaseOrder>(purchasingApiPaths.purchaseOrderSubmit(id), jsonRequest("PATCH", {}));
}

function jsonRequest(method: "PATCH" | "POST" | "PUT", body: object): RequestInit {
  return { method, headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) };
}
