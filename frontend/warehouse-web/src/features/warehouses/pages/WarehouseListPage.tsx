import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import { useWarehouses } from "../api/useWarehouses";
import type { Warehouse } from "../api/warehouseTypes";
import { warehouseRoutes } from "../warehouseConstants";
import {
  ListPageLayout,
  ListFilter,
  ReturnAwareLink,
  NewPageAction,
} from "../../../shared/components/PageLayouts";

export function WarehouseListPage() {
  const listQuery = useUrlListQuery();
  const { t } = useTranslation();
  const activeValue = listQuery.get("active");
  const { data, error, isLoading, isFetching } = useWarehouses({
    ...listQuery.request,
    search: listQuery.get("q"),
    isActive:
      activeValue === "true"
        ? true
        : activeValue === "false"
          ? false
          : undefined,
  });

  const columns = useMemo<ColumnsType<Warehouse>>(
    () => [
      { title: t("warehouses.table.code"), dataIndex: "code", key: "code" },
      { title: t("warehouses.table.name"), dataIndex: "name", key: "name" },
      {
        title: t("warehouses.table.status"),
        dataIndex: "isActive",
        key: "isActive",
        render: (isActive: boolean) => <ProductStatusTag isActive={isActive} />,
      },
      {
        title: t("warehouses.table.actions"),
        key: "actions",
        fixed: "right",
        width: 120,
        render: (_, warehouse) => (
          <ReturnAwareLink to={warehouseRoutes.detail(warehouse.id)}>
            {t("warehouses.view")}
          </ReturnAwareLink>
        ),
      },
    ],
    [t],
  );

  return (
    <ListPageLayout
      actions={<NewPageAction to={warehouseRoutes.create} />}
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <>
          <ListFilter label={t("ui.search")} width="search">
            <Input.Search
              key={listQuery.get("q") ?? "q"}
              allowClear
              defaultValue={listQuery.get("q")}
              onSearch={(value) => listQuery.update({ q: value })}
              placeholder={t("warehouses.searchPlaceholder")}
            />
          </ListFilter>
          <ListFilter label={t("warehouses.table.status")} width="compact">
            <Select
              allowClear
              aria-label={t("warehouses.table.status")}
              onChange={(value) => listQuery.update({ active: value })}
              options={[
                { value: "true", label: t("products.status.active") },
                { value: "false", label: t("products.status.inactive") },
              ]}
              placeholder={t("warehouses.table.status")}
              value={activeValue}
            />
          </ListFilter>
        </>
      }
      subtitle={t("warehouses.subtitle")}
      title={t("warehouses.title")}
    >
      {isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("warehouses.loadingList")}
        />
      ) : null}
      {error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, error, "warehouses.errors.loadList")}
          showIcon
          type="error"
        />
      ) : null}
      {data && data.items.length === 0 ? (
        <Empty description={t("warehouses.empty")} />
      ) : null}
      {data && data.items.length > 0 ? (
        <Table
          columns={columns}
          dataSource={data.items}
          loading={isFetching}
          pagination={listQuery.toTablePagination(data)}
          rowKey="id"
          scroll={{ x: 700 }}
        />
      ) : null}
    </ListPageLayout>
  );
}
