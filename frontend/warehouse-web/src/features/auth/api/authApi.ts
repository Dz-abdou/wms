import { apiUrl, requestJson } from "../../../shared/api/apiClient";
export type Session = { id: string; email: string; roles: string[] };
let accessToken: string | undefined;
let refreshPromise: Promise<void> | undefined;
export function getAccessToken() {
  return accessToken;
}
export function clearAccessToken() {
  accessToken = undefined;
}
export async function login(email: string, password: string) {
  const result = await requestJson<{ accessToken: string }>("/api/auth/login", {
    method: "POST",
    retryOnUnauthorized: false,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
  accessToken = result.accessToken;
}
export function refreshAccessToken(): Promise<void> {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = requestJson<{ accessToken: string }>("/api/auth/refresh", {
    method: "POST",
    retryOnUnauthorized: false,
  })
    .then((result) => {
      accessToken = result.accessToken;
    })
    .finally(() => {
      refreshPromise = undefined;
    });

  return refreshPromise;
}
export function getSession() {
  return requestJson<Session>("/api/auth/me");
}
export async function logout() {
  await fetch(apiUrl("/api/auth/logout"), {
    method: "POST",
    credentials: "include",
  });
}
