import type { TablePaginationConfig } from "antd";
import { useCallback, useState } from "react";

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

export function useListPagination(
  initial: PageRequest = {
    page: paginationDefaults.defaultPage,
    pageSize: paginationDefaults.defaultPageSize,
  },
) {
  const [page, setPage] = useState(initial.page);
  const [pageSize, setPageSize] = useState(initial.pageSize);

  const resetPage = useCallback(
    () => setPage(paginationDefaults.defaultPage),
    [],
  );

  const toTablePagination = useCallback(
    (result: PagedResult<unknown>): TablePaginationConfig => ({
      current: result.page,
      pageSize: result.pageSize,
      total: result.totalCount,
      showSizeChanger: true,
      onChange: (nextPage, nextPageSize) => {
        setPage(nextPage);
        setPageSize(nextPageSize);
      },
    }),
    [],
  );

  return {
    page,
    pageSize,
    request: { page, pageSize },
    resetPage,
    toTablePagination,
  };
}
