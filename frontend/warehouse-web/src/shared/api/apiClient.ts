export type ProblemDetails = {
  code?: string
  detail?: string
  errorCodes?: Record<string, string[]>
  errors?: Record<string, string[]>
  status?: number
  title?: string
  type?: string
}

export class ApiError extends Error {
  public constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails
  ) {
    super(problem.title ?? 'The request failed.')
  }
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') ?? ''

export function apiUrl(path: string): string {
  return `${apiBaseUrl}${path}`
}

export async function requestJson<TResponse>(path: string, init?: RequestInit): Promise<TResponse> {
  const response = await fetch(apiUrl(path), {
    ...init,
    headers: {
      Accept: 'application/json',
      ...init?.headers
    }
  })

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as ProblemDetails
    throw new ApiError(response.status, problem)
  }

  return (await response.json()) as TResponse
}