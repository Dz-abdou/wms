import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useSuppliers } from "../api/useSuppliers";
import type { Supplier } from "../api/supplierTypes";
import { supplierRoutes } from "../supplierConstants";
import {
  ListPageLayout,
  ListFilter,
  ReturnAwareLink,
  NewPageAction,
} from "../../../shared/components/PageLayouts";

export function SupplierListPage() {
  const listQuery = useUrlListQuery();
  const { t } = useTranslation();
  const activeValue = listQuery.get("active");
  const { data, error, isLoading, isFetching } = useSuppliers({
    ...listQuery.request,
    search: listQuery.get("q"),
    isActive:
      activeValue === "true"
        ? true
        : activeValue === "false"
          ? false
          : undefined,
  });

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
      actions={<NewPageAction to={supplierRoutes.create} />}
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
              placeholder={t("suppliers.searchPlaceholder")}
            />
          </ListFilter>
          <ListFilter label={t("suppliers.table.status")} width="compact">
            <Select
              allowClear
              aria-label={t("suppliers.table.status")}
              onChange={(value) => listQuery.update({ active: value })}
              options={[
                { value: "true", label: t("products.status.active") },
                { value: "false", label: t("products.status.inactive") },
              ]}
              placeholder={t("suppliers.table.status")}
              value={activeValue}
            />
          </ListFilter>
        </>
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
          pagination={listQuery.toTablePagination(data)}
          rowKey="id"
        />
      ) : null}
    </ListPageLayout>
  );
}
