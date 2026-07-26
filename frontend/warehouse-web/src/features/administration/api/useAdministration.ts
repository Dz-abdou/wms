import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createUser,
  deleteUser,
  getUser,
  getRoles,
  getUsers,
  setUserRoles,
  updateUser,
} from "./administrationApi";
import type {
  AdministrationUserListQuery,
  CreateUserValues,
  UpdateUserValues,
} from "./administrationTypes";

export const administrationKeys = {
  all: ["administration"] as const,
  roles: () => [...administrationKeys.all, "roles"] as const,
  users: (query: AdministrationUserListQuery) =>
    [...administrationKeys.all, "users", query] as const,
  user: (id: string) => [...administrationKeys.all, "user", id] as const,
};

export function useAdministrationUsers(query: AdministrationUserListQuery) {
  return useQuery({
    queryKey: administrationKeys.users(query),
    queryFn: ({ signal }) => getUsers(query, signal),
  });
}

export function useAdministrationUser(id: string | undefined) {
  return useQuery({
    queryKey: administrationKeys.user(id ?? ""),
    queryFn: ({ signal }) => getUser(id ?? "", signal),
    enabled: Boolean(id),
  });
}

export function useAdministrationRoles() {
  return useQuery({
    queryKey: administrationKeys.roles(),
    queryFn: ({ signal }) => getRoles(signal),
  });
}

export function useCreateAdministrationUser() {
  return useAdministrationMutation((values: CreateUserValues) =>
    createUser(values),
  );
}

export function useUpdateAdministrationUser() {
  return useAdministrationMutation(
    ({ id, values }: { id: string; values: UpdateUserValues }) =>
      updateUser(id, values),
  );
}

export function useDeleteAdministrationUser() {
  return useAdministrationMutation((id: string) => deleteUser(id));
}

export function useSetAdministrationUserRoles() {
  return useAdministrationMutation(
    ({ id, roles }: { id: string; roles: string[] }) => setUserRoles(id, roles),
  );
}

function useAdministrationMutation<TValues, TResult>(
  mutationFn: (values: TValues) => Promise<TResult>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: administrationKeys.all }),
  });
}
