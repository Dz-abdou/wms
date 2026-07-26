import { Alert, Empty, Select, Spin, Table, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { usePurchaseOrders } from "../api/usePurchasing";
import { useSuppliers } from "../../suppliers/api/useSuppliers";
import type { PurchaseOrder } from "../api/purchasingTypes";
import { purchasingRoutes } from "../purchasingConstants";
import {
  ListFilter,
  ListPageLayout,
  ReturnAwareLink,
  NewPageAction,
} from "../../../shared/components/PageLayouts";

export function PurchaseOrderListPage() {
  const { t } = useTranslation();
  const listQuery = useUrlListQuery();
  const [supplierSearch, setSupplierSearch] = useState("");
  const status = listQuery.get("status");
  const orders = usePurchaseOrders({
    ...listQuery.request,
    supplierId: listQuery.get("supplierId"),
    status: status === "0" ? 0 : status === "1" ? 1 : undefined,
  });
  const suppliers = useSuppliers({
    page: 1,
    pageSize: 20,
    search: supplierSearch,
  });
  const columns = useMemo<ColumnsType<PurchaseOrder>>(
    () => [
      {
        title: t("purchasing.orders.supplier"),
        key: "supplier",
        render: (_, item) => `${item.supplierCode} — ${item.supplierName}`,
      },
      {
        title: t("purchasing.orders.status"),
        key: "status",
        render: (_, item) => (
          <Tag color={item.status === 0 ? "gold" : "green"}>
            {t(
              item.status === 0
                ? "purchasing.status.draft"
                : "purchasing.status.submitted",
            )}
          </Tag>
        ),
      },
      {
        title: t("purchasing.orders.actions"),
        key: "actions",
        render: (_, item) => (
          <ReturnAwareLink to={purchasingRoutes.orderDetail(item.id)}>
            {t("purchasing.view")}
          </ReturnAwareLink>
        ),
      },
    ],
    [t],
  );
  return (
    <ListPageLayout
      actions={<NewPageAction to={purchasingRoutes.orderCreate} />}
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <>
          <ListFilter label={t("purchasing.orders.supplier")} width="regular">
            <Select
              allowClear
              aria-label={t("purchasing.orders.supplier")}
              filterOption={false}
              onChange={(value) => listQuery.update({ supplierId: value })}
              onSearch={setSupplierSearch}
              options={(suppliers.data?.items ?? []).map((supplier) => ({
                value: supplier.id,
                label: `${supplier.code} — ${supplier.name}`,
              }))}
              placeholder={t("purchasing.orders.supplier")}
              showSearch
              value={listQuery.get("supplierId")}
            />
          </ListFilter>
          <ListFilter label={t("purchasing.orders.status")} width="compact">
            <Select
              allowClear
              aria-label={t("purchasing.orders.status")}
              onChange={(value) => listQuery.update({ status: value })}
              options={[
                { value: "0", label: t("purchasing.status.draft") },
                { value: "1", label: t("purchasing.status.submitted") },
              ]}
              placeholder={t("purchasing.orders.status")}
              value={status}
            />
          </ListFilter>
        </>
      }
      subtitle={t("purchasing.orders.subtitle")}
      title={t("purchasing.orders.title")}
    >
      {orders.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("purchasing.orders.loading")}
        />
      ) : null}
      {orders.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            orders.error,
            "purchasing.orders.errors.load",
          )}
          showIcon
          type="error"
        />
      ) : null}
      {orders.data && orders.data.items.length === 0 ? (
        <Empty description={t("purchasing.orders.empty")} />
      ) : null}
      {orders.data && orders.data.items.length > 0 ? (
        <Table
          columns={columns}
          dataSource={orders.data.items}
          loading={orders.isFetching}
          pagination={listQuery.toTablePagination(orders.data)}
          rowKey="id"
        />
      ) : null}
    </ListPageLayout>
  );
}
