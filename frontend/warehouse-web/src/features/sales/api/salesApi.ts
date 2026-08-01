import { requestJson } from "../../../shared/api/apiClient";
import { salesApiPaths } from "../salesConstants";
import type {
  SalesOrder,
  SalesOrderInput,
  SalesOrderListQuery,
  SalesOrderListResult,
} from "./salesTypes";

export function getSalesOrders(
  query: SalesOrderListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.status) parameters.set("status", query.status);
  if (query.customerId) parameters.set("customerId", query.customerId);
  if (query.fromOrderDate) parameters.set("fromOrderDate", query.fromOrderDate);
  if (query.toOrderDate) parameters.set("toOrderDate", query.toOrderDate);
  return requestJson<SalesOrderListResult>(
    `${salesApiPaths.orders}?${parameters}`,
    { signal },
  );
}
export function getSalesOrder(id: string, signal?: AbortSignal) {
  return requestJson<SalesOrder>(salesApiPaths.orderById(id), { signal });
}
export function createSalesOrder(input: SalesOrderInput) {
  return requestJson<SalesOrder>(
    salesApiPaths.orders,
    jsonRequest("POST", input),
  );
}
export function updateSalesOrder(id: string, input: SalesOrderInput) {
  return requestJson<SalesOrder>(
    salesApiPaths.orderById(id),
    jsonRequest("PUT", input),
  );
}
export function submitSalesOrder(id: string, version: number) {
  return requestJson<SalesOrder>(
    salesApiPaths.submit(id),
    jsonRequest("PATCH", { version }),
  );
}
export function cancelSalesOrder(id: string, version: number, reason?: string) {
  return requestJson<SalesOrder>(
    salesApiPaths.cancel(id),
    jsonRequest("PATCH", { version, reason }),
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
