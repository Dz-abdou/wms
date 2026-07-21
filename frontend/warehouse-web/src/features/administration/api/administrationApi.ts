import { requestJson } from "../../../shared/api/apiClient";
import { administrationApiPaths } from "../administrationConstants";
import type {
  AdministrationUser,
  CreateUserValues,
  UpdateUserValues,
} from "./administrationTypes";

export function getUsers(signal?: AbortSignal) {
  return requestJson<AdministrationUser[]>(administrationApiPaths.users, {
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
