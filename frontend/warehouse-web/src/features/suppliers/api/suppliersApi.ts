import { requestJson } from "../../../shared/api/apiClient";
import { supplierApiPaths } from "../supplierConstants";
import type {
  Supplier,
  SupplierInput,
  SupplierListQuery,
  SupplierListResult,
} from "./supplierTypes";

export function getSuppliers(query: SupplierListQuery, signal?: AbortSignal) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.search?.trim()) parameters.set("search", query.search.trim());
  if (query.isActive !== undefined)
    parameters.set("isActive", String(query.isActive));
  return requestJson<SupplierListResult>(
    `${supplierApiPaths.base}?${parameters}`,
    { signal },
  );
}

export function getSupplier(id: string, signal?: AbortSignal) {
  return requestJson<Supplier>(supplierApiPaths.byId(id), { signal });
}

export function createSupplier(input: SupplierInput) {
  return requestJson<Supplier>(supplierApiPaths.base, request("POST", input));
}

export function updateSupplier(id: string, input: SupplierInput) {
  return requestJson<Supplier>(
    supplierApiPaths.byId(id),
    request("PUT", input),
  );
}

export function setSupplierStatus(id: string, isActive: boolean) {
  return requestJson<Supplier>(
    supplierApiPaths.status(id),
    request("PATCH", { isActive }),
  );
}

function request(method: "PATCH" | "POST" | "PUT", body: object): RequestInit {
  return {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  };
}
