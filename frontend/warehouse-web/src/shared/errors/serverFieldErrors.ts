import type { FormInstance } from "antd";
import type { TFunction } from "i18next";
import { ApiError } from "../api/apiClient";
import { getFieldErrorMessages } from "./problemDetails";

type FieldName = string | number | Array<string | number>;

export function hasServerFieldErrors(error: unknown): error is ApiError {
  return (
    error instanceof ApiError &&
    Boolean(
      error.problem.errors && Object.keys(error.problem.errors).length > 0,
    )
  );
}

export function applyServerFieldErrors(
  form: FormInstance,
  error: unknown,
  t: TFunction,
  fallbackKey: string,
  resolveFieldName: (property: string) => FieldName | undefined = toFieldName,
): boolean {
  if (!hasServerFieldErrors(error)) {
    return false;
  }

  const fields = Object.entries(error.problem.errors ?? {})
    .map(([property]) => {
      const name = resolveFieldName(property);

      return name
        ? {
            name,
            errors: getFieldErrorMessages(
              t,
              getErrorCodes(error.problem.errorCodes, property),
              fallbackKey,
            ),
          }
        : undefined;
    })
    .filter(
      (field): field is { name: FieldName; errors: string[] } =>
        field !== undefined,
    );

  if (fields.length === 0) {
    return false;
  }

  form.setFields(fields);
  return true;
}

function toFieldName(property: string): FieldName | undefined {
  if (!property) {
    return undefined;
  }

  const nestedProperty = /^(?<list>[^[]+)\[(?<index>\d+)]\.(?<field>.+)$/.exec(
    property,
  )?.groups;
  if (nestedProperty) {
    return [
      toCamelCase(nestedProperty.list),
      Number(nestedProperty.index),
      toCamelCase(nestedProperty.field),
    ];
  }

  if (property.includes(".")) {
    return property.split(".").map(toCamelCase);
  }

  return toCamelCase(property);
}

function toCamelCase(property: string): string {
  return property.charAt(0).toLowerCase() + property.slice(1);
}

function getErrorCodes(
  errorCodes: Record<string, string[]> | undefined,
  property: string,
): string[] | undefined {
  if (!errorCodes) {
    return undefined;
  }

  const matchingProperty = Object.keys(errorCodes).find(
    (key) => key.toLowerCase() === property.toLowerCase(),
  );
  return matchingProperty ? errorCodes[matchingProperty] : undefined;
}
