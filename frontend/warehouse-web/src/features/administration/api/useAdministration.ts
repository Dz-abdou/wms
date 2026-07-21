import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createUser,
  deleteUser,
  getRoles,
  getUsers,
  setUserRoles,
  updateUser,
} from "./administrationApi";
import type { CreateUserValues, UpdateUserValues } from "./administrationTypes";

export const administrationKeys = {
  all: ["administration"] as const,
  roles: () => [...administrationKeys.all, "roles"] as const,
  users: () => [...administrationKeys.all, "users"] as const,
};

export function useAdministrationUsers() {
  return useQuery({
    queryKey: administrationKeys.users(),
    queryFn: ({ signal }) => getUsers(signal),
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
