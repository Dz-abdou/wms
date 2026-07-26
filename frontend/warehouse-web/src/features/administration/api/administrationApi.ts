import { requestJson } from "../../../shared/api/apiClient";
import { administrationApiPaths } from "../administrationConstants";
import type {
  AdministrationUser,
  AdministrationUserListQuery,
  AdministrationUserListResult,
  CreateUserValues,
  UpdateUserValues,
} from "./administrationTypes";

export function getUsers(
  query: AdministrationUserListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.email?.trim()) parameters.set("email", query.email.trim());
  if (query.role) parameters.set("role", query.role);
  return requestJson<AdministrationUserListResult>(
    `${administrationApiPaths.users}?${parameters}`,
    {
      signal,
    },
  );
}

export function getUser(id: string, signal?: AbortSignal) {
  return requestJson<AdministrationUser>(administrationApiPaths.user(id), {
    signal,
  });
}

export function getRoles(signal?: AbortSignal) {
  return requestJson<string[]>(administrationApiPaths.roles, { signal });
}

export function createUser(values: CreateUserValues) {
  return requestJson<AdministrationUser>(
    administrationApiPaths.users,
    jsonRequest("POST", values),
  );
}

export function updateUser(id: string, values: UpdateUserValues) {
  return requestJson<AdministrationUser>(
    administrationApiPaths.user(id),
    jsonRequest("PUT", values),
  );
}

export function deleteUser(id: string) {
  return requestJson<void>(administrationApiPaths.user(id), {
    method: "DELETE",
  });
}

export function setUserRoles(id: string, roles: string[]) {
  return requestJson<AdministrationUser>(
    administrationApiPaths.userRoles(id),
    jsonRequest("PUT", { roles }),
  );
}

function jsonRequest(method: "POST" | "PUT", body: object): RequestInit {
  return {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  };
}
