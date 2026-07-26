import { Alert, Empty, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useListPagination } from "../../../shared/pagination/pagination";
import { useSuppliers } from "../api/useSuppliers";
import type { Supplier } from "../api/supplierTypes";
import { supplierRoutes } from "../supplierConstants";
import {
  ListPageLayout,
  ReturnAwareLink,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";

export function SupplierListPage() {
  const pagination = useListPagination();
  const { t } = useTranslation();
  const { data, error, isLoading, isFetching } = useSuppliers(
    pagination.page,
    pagination.pageSize,
  );

  const columns = useMemo<ColumnsType<Supplier>>(
    () => [
      { title: t("suppliers.table.code"), dataIndex: "code", key: "code" },
      { title: t("suppliers.table.name"), dataIndex: "name", key: "name" },
      {
        title: t("suppliers.table.status"),
        dataIndex: "isActive",
        key: "isActive",
        render: (isActive: boolean) => <ProductStatusTag isActive={isActive} />,
      },
      {
        title: t("suppliers.table.actions"),
        key: "actions",
        render: (_, supplier) => (
          <ReturnAwareLink to={supplierRoutes.detail(supplier.id)}>
            {t("suppliers.view")}
          </ReturnAwareLink>
        ),
      },
    ],
    [t],
  );

  return (
    <ListPageLayout
      actions={
        <RouteActionButton to={supplierRoutes.create} type="primary">
          {t("suppliers.new")}
        </RouteActionButton>
      }
      subtitle={t("suppliers.subtitle")}
      title={t("suppliers.title")}
    >

      {isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("suppliers.loadingList")}
        />
      ) : null}
      {error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, error, "suppliers.errors.loadList")}
          showIcon
          type="error"
        />
      ) : null}
      {data && data.items.length === 0 ? (
        <Empty description={t("suppliers.empty")} />
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
