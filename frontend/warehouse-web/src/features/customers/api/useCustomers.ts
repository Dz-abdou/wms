import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createCustomer,
  createCustomerAddress,
  createCustomerContact,
  deleteCustomerAddress,
  deleteCustomerContact,
  getCustomer,
  getCustomers,
  setCustomerStatus,
  updateCustomer,
  updateCustomerAddress,
  updateCustomerContact,
} from "./customersApi";
import type {
  Customer,
  CustomerAddressInput,
  CustomerContactInput,
  CustomerInput,
  CustomerListQuery,
} from "./customerTypes";

export const customerKeys = {
  all: ["customers"] as const,
  list: (query: CustomerListQuery) =>
    [...customerKeys.all, "list", query] as const,
  detail: (id: string) => [...customerKeys.all, "detail", id] as const,
};

export function useCustomers(query: CustomerListQuery) {
  return useQuery({
    queryKey: customerKeys.list(query),
    queryFn: ({ signal }) => getCustomers(query, signal),
  });
}

export function useCustomer(id: string | undefined) {
  return useQuery({
    queryKey: customerKeys.detail(id ?? ""),
    queryFn: ({ signal }) => getCustomer(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useCreateCustomer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createCustomer,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: customerKeys.all }),
  });
}

export function useUpdateCustomer(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CustomerInput) => updateCustomer(id, input),
    onSuccess: (customer) => refresh(queryClient, customer),
  });
}

export function useSetCustomerStatus(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (isActive: boolean) => setCustomerStatus(id, isActive),
    onSuccess: (customer) => refresh(queryClient, customer),
  });
}

export function useCreateCustomerContact(id: string) {
  return useCustomerMutation((input: CustomerContactInput) =>
    createCustomerContact(id, input),
  );
}

export function useUpdateCustomerContact(id: string, contactId: string) {
  return useCustomerMutation((input: CustomerContactInput) =>
    updateCustomerContact(id, contactId, input),
  );
}

export function useDeleteCustomerContact(id: string) {
  return useCustomerMutation((contactId: string) =>
    deleteCustomerContact(id, contactId),
  );
}

export function useCreateCustomerAddress(id: string) {
  return useCustomerMutation((input: CustomerAddressInput) =>
    createCustomerAddress(id, input),
  );
}

export function useUpdateCustomerAddress(id: string, addressId: string) {
  return useCustomerMutation((input: CustomerAddressInput) =>
    updateCustomerAddress(id, addressId, input),
  );
}

export function useDeleteCustomerAddress(id: string) {
  return useCustomerMutation((addressId: string) =>
    deleteCustomerAddress(id, addressId),
  );
}

function useCustomerMutation<TInput>(
  mutationFn: (input: TInput) => Promise<unknown>,
) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: customerKeys.all }),
  });
}

function refresh(
  queryClient: ReturnType<typeof useQueryClient>,
  customer: Customer,
) {
  queryClient.setQueryData(customerKeys.detail(customer.id), customer);
  return queryClient.invalidateQueries({ queryKey: customerKeys.all });
}
