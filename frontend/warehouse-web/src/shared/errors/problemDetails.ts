import type { TFunction } from 'i18next'
import { ApiError, type ProblemDetails } from '../api/apiClient'

export function getProblemMessage(t: TFunction, problem: ProblemDetails, fallbackKey: string): string {
  const translatedCode = problem.code ? translateErrorCode(t, problem.code) : undefined
  return translatedCode ?? t(fallbackKey)
}

export function getErrorMessage(t: TFunction, error: unknown, fallbackKey: string): string {
  return error instanceof ApiError ? getProblemMessage(t, error.problem, fallbackKey) : t(fallbackKey)
}

export function getFieldErrorMessages(
  t: TFunction,
  errorCodes: string[] | undefined,
  fallbackKey: string): string[] {
  const translated = errorCodes
    ?.map((errorCode) => translateErrorCode(t, errorCode))
    .filter((message): message is string => message !== undefined)

  return translated && translated.length > 0 ? translated : [t(fallbackKey)]
}

function translateErrorCode(t: TFunction, errorCode: string): string | undefined {
  const translation = t(`errors.codes.${errorCode}`, { defaultValue: '' })
  return translation || undefined
}