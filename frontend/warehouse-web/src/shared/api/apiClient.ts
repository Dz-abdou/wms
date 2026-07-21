export type ProblemDetails = {
  code?: string;
  detail?: string;
  errorCodes?: Record<string, string[]>;
  errors?: Record<string, string[]>;
  status?: number;
  title?: string;
  type?: string;
};

export class ApiError extends Error {
  public constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails,
  ) {
    super(problem.title ?? "The request failed.");
  }
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "";

type ApiRequestInit = RequestInit & { retryOnUnauthorized?: boolean };

let authentication:
  | {
      getAccessToken: () => string | undefined;
      refreshAccessToken: () => Promise<boolean>;
    }
  | undefined;

export function configureAuthentication(value: typeof authentication) {
  authentication = value;
}

export function apiUrl(path: string): string {
  return `${apiBaseUrl}${path}`;
}

export async function requestJson<TResponse>(
  path: string,
  init?: ApiRequestInit,
): Promise<TResponse> {
  const { retryOnUnauthorized = true, ...requestInit } = init ?? {};
  let response = await send(path, requestInit);

  if (
    response.status === 401 &&
    retryOnUnauthorized &&
    authentication &&
    (await authentication.refreshAccessToken())
  ) {
    response = await send(path, requestInit);
  }

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as ProblemDetails;
    throw new ApiError(response.status, problem);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return (await response.json()) as TResponse;
}

function send(path: string, init: RequestInit): Promise<Response> {
  const headers = new Headers(init.headers);
  headers.set("Accept", "application/json");
  const token = authentication?.getAccessToken();
  if (token && !headers.has("Authorization"))
    headers.set("Authorization", `Bearer ${token}`);
  return fetch(apiUrl(path), {
    ...init,
    credentials: "include",
    headers: {
      ...Object.fromEntries(headers),
    },
  });
}
