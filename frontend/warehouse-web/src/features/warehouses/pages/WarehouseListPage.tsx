import { Alert, Empty, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useListPagination } from "../../../shared/pagination/pagination";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import { useWarehouses } from "../api/useWarehouses";
import type { Warehouse } from "../api/warehouseTypes";
import { warehouseRoutes } from "../warehouseConstants";
import {
  ListPageLayout,
  ReturnAwareLink,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";

export function WarehouseListPage() {
  const pagination = useListPagination();
  const { t } = useTranslation();
  const { data, error, isLoading, isFetching } = useWarehouses(
    pagination.page,
    pagination.pageSize,
  );

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
      actions={
        <RouteActionButton to={warehouseRoutes.create} type="primary">
          {t("warehouses.new")}
        </RouteActionButton>
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
          pagination={pagination.toTablePagination(data)}
          rowKey="id"
        />
      ) : null}
    </ListPageLayout>
  );
}
