import { requestJson } from "../../../shared/api/apiClient";
import { customerApiPaths } from "../customerConstants";
import type {
  Customer,
  CustomerAddress,
  CustomerAddressInput,
  CustomerContact,
  CustomerContactInput,
  CustomerInput,
  CustomerListQuery,
  CustomerListResult,
} from "./customerTypes";

export function getCustomers(query: CustomerListQuery, signal?: AbortSignal) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.search?.trim()) parameters.set("search", query.search.trim());
  if (query.isActive !== undefined) {
    parameters.set("isActive", String(query.isActive));
  }
  return requestJson<CustomerListResult>(
    `${customerApiPaths.base}?${parameters}`,
    { signal },
  );
}

export function getCustomer(id: string, signal?: AbortSignal) {
  return requestJson<Customer>(customerApiPaths.byId(id), { signal });
}

export function createCustomer(input: CustomerInput) {
  return requestJson<Customer>(customerApiPaths.base, request("POST", input));
}

export function updateCustomer(id: string, input: CustomerInput) {
  return requestJson<Customer>(
    customerApiPaths.byId(id),
    request("PUT", input),
  );
}

export function setCustomerStatus(id: string, isActive: boolean) {
  return requestJson<Customer>(
    customerApiPaths.status(id),
    request("PATCH", { isActive }),
  );
}

export function createCustomerContact(id: string, input: CustomerContactInput) {
  return requestJson<CustomerContact>(
    customerApiPaths.contacts(id),
    request("POST", input),
  );
}

export function updateCustomerContact(
  id: string,
  contactId: string,
  input: CustomerContactInput,
) {
  return requestJson<CustomerContact>(
    customerApiPaths.contact(id, contactId),
    request("PUT", input),
  );
}

export function deleteCustomerContact(id: string, contactId: string) {
  return requestJson<void>(customerApiPaths.contact(id, contactId), {
    method: "DELETE",
  });
}

export function createCustomerAddress(id: string, input: CustomerAddressInput) {
  return requestJson<CustomerAddress>(
    customerApiPaths.addresses(id),
    request("POST", input),
  );
}

export function updateCustomerAddress(
  id: string,
  addressId: string,
  input: CustomerAddressInput,
) {
  return requestJson<CustomerAddress>(
    customerApiPaths.address(id, addressId),
    request("PUT", input),
  );
}

export function deleteCustomerAddress(id: string, addressId: string) {
  return requestJson<void>(customerApiPaths.address(id, addressId), {
    method: "DELETE",
  });
}

function request(method: "PATCH" | "POST" | "PUT", body: object): RequestInit {
  return {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  };
}
