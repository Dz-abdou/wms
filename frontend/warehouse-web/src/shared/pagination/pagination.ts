import type { TablePaginationConfig } from "antd";
import { useCallback } from "react";
import { useSearchParams } from "react-router-dom";

export const paginationDefaults = {
  defaultPage: 1,
  defaultPageSize: 20,
} as const;

export type PageRequest = {
  page: number;
  pageSize: number;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type ListQueryValue = boolean | number | string | null | undefined;

const pageParameter = "page";
const pageSizeParameter = "pageSize";

function parsePositiveInteger(
  value: string | null,
  fallback: number,
  maximum: number,
) {
  const parsed = Number(value);
  return Number.isInteger(parsed) && parsed > 0 && parsed <= maximum
    ? parsed
    : fallback;
}

/**
 * Keeps list pagination and feature filters in the URL so a refresh, shared
 * link, or return navigation preserves the exact server query.
 */
export function useUrlListQuery() {
  const [searchParams, setSearchParams] = useSearchParams();
  const page = parsePositiveInteger(
    searchParams.get(pageParameter),
    paginationDefaults.defaultPage,
    1_000_000,
  );
  const pageSize = parsePositiveInteger(
    searchParams.get(pageSizeParameter),
    paginationDefaults.defaultPageSize,
    100,
  );

  const update = useCallback(
    (changes: Record<string, ListQueryValue>, resetPage = true) => {
      setSearchParams((current) => {
        const next = new URLSearchParams(current);
        for (const [key, value] of Object.entries(changes)) {
          if (value === null || value === undefined || value === "") {
            next.delete(key);
          } else {
            next.set(key, String(value));
          }
        }

        if (resetPage) {
          next.delete(pageParameter);
        }

        return next;
      });
    },
    [setSearchParams],
  );

  const toTablePagination = useCallback(
    (result: PagedResult<unknown>): TablePaginationConfig => ({
      current: result.page,
      pageSize: result.pageSize,
      total: result.totalCount,
      showSizeChanger: true,
      onChange: (nextPage, nextPageSize) => {
        update(
          {
            [pageParameter]:
              nextPage === paginationDefaults.defaultPage ? null : nextPage,
            [pageSizeParameter]:
              nextPageSize === paginationDefaults.defaultPageSize
                ? null
                : nextPageSize,
          },
          false,
        );
      },
    }),
    [update],
  );

  const hasFilters = Array.from(searchParams.keys()).some(
    (key) => key !== pageParameter && key !== pageSizeParameter,
  );

  const clearFilters = useCallback(() => {
    setSearchParams((current) => {
      const next = new URLSearchParams();
      const pageSize = current.get(pageSizeParameter);
      if (pageSize) {
        next.set(pageSizeParameter, pageSize);
      }

      return next;
    });
  }, [setSearchParams]);

  return {
    page,
    pageSize,
    get: (key: string) => searchParams.get(key) ?? undefined,
    hasFilters,
    request: { page, pageSize },
    toTablePagination,
    update,
    clearFilters,
  };
}

export function useListPagination(
  initial: PageRequest = {
    page: paginationDefaults.defaultPage,
    pageSize: paginationDefaults.defaultPageSize,
  },
) {
  const query = useUrlListQuery();

  return {
    ...query,
    page: query.get(pageParameter) ? query.page : initial.page,
    pageSize: query.get(pageSizeParameter) ? query.pageSize : initial.pageSize,
    request: {
      page: query.get(pageParameter) ? query.page : initial.page,
      pageSize: query.get(pageSizeParameter)
        ? query.pageSize
        : initial.pageSize,
    },
    resetPage: () => query.update({}, true),
  };
}
