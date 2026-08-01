import { Alert, Empty, Input, Select, Spin, Table, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { usePurchaseOrders } from "../api/usePurchasing";
import { useSuppliers } from "../../suppliers/api/useSuppliers";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import {
  purchaseOrderStatusColors,
  purchaseOrderStatusTranslationKeys,
  type PurchaseOrder,
} from "../api/purchasingTypes";
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
  const [warehouseSearch, setWarehouseSearch] = useState("");
  const status = listQuery.get("status");
  const orders = usePurchaseOrders({
    ...listQuery.request,
    supplierId: listQuery.get("supplierId"),
    status: status as PurchaseOrder["status"] | undefined,
    warehouseId: listQuery.get("warehouseId"),
    fromOrderDate: listQuery.get("fromOrderDate"),
    toOrderDate: listQuery.get("toOrderDate"),
  });
  const warehouses = useWarehouses({
    page: 1,
    pageSize: 20,
    search: warehouseSearch,
  });
  const suppliers = useSuppliers({
    page: 1,
    pageSize: 20,
    search: supplierSearch,
  });
  const columns = useMemo<ColumnsType<PurchaseOrder>>(
    () => [
      {
        title: t("purchasing.orders.number"),
        dataIndex: "number",
        key: "number",
      },
      {
        title: t("purchasing.orders.warehouse"),
        key: "warehouse",
        render: (_, item) =>
          item.destinationWarehouseCode
            ? `${item.destinationWarehouseCode} — ${item.destinationWarehouseName}`
            : "—",
      },
      {
        title: t("purchasing.orders.orderDate"),
        dataIndex: "orderDate",
        key: "orderDate",
      },
      {
        title: t("purchasing.orders.expectedDeliveryDate"),
        dataIndex: "expectedDeliveryDate",
        key: "expectedDeliveryDate",
        render: (value: string | undefined) => value ?? "—",
      },
      {
        title: t("purchasing.orders.lineCount"),
        key: "lineCount",
        render: (_, item) => item.lines.length,
      },
      {
        title: t("purchasing.orders.total"),
        key: "total",
        render: (_, item) => `${item.totalAmount} ${item.currencyCode ?? ""}`,
      },
      {
        title: t("purchasing.orders.supplier"),
        key: "supplier",
        render: (_, item) => `${item.supplierCode} — ${item.supplierName}`,
      },
      {
        title: t("purchasing.orders.status"),
        key: "status",
        render: (_, item) => (
          <Tag color={purchaseOrderStatusColors[item.status]}>
            {t(purchaseOrderStatusTranslationKeys[item.status])}
          </Tag>
        ),
      },
      {
        title: t("purchasing.orders.actions"),
        key: "actions",
        fixed: "right",
        width: 120,
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
                { value: "Draft", label: t("purchasing.status.draft") },
                { value: "Submitted", label: t("purchasing.status.submitted") },
                {
                  value: "PartiallyReceived",
                  label: t("purchasing.status.partiallyReceived"),
                },
                { value: "Received", label: t("purchasing.status.received") },
                { value: "Cancelled", label: t("purchasing.status.cancelled") },
              ]}
              placeholder={t("purchasing.orders.status")}
              value={status}
            />
          </ListFilter>
          <ListFilter label={t("purchasing.orders.warehouse")} width="regular">
            <Select
              allowClear
              aria-label={t("purchasing.orders.warehouse")}
              filterOption={false}
              onChange={(value) => listQuery.update({ warehouseId: value })}
              onSearch={setWarehouseSearch}
              options={(warehouses.data?.items ?? []).map((warehouse) => ({
                value: warehouse.id,
                label: `${warehouse.code} — ${warehouse.name}`,
              }))}
              placeholder={t("purchasing.orders.warehouse")}
              showSearch
              value={listQuery.get("warehouseId")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.filters.fromDate")} width="compact">
            <Input
              type="date"
              value={listQuery.get("fromOrderDate")}
              onChange={(event) =>
                listQuery.update({
                  fromOrderDate: event.target.value || undefined,
                })
              }
            />
          </ListFilter>
          <ListFilter label={t("inventory.filters.toDate")} width="compact">
            <Input
              type="date"
              value={listQuery.get("toOrderDate")}
              onChange={(event) =>
                listQuery.update({
                  toOrderDate: event.target.value || undefined,
                })
              }
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
          scroll={{ x: 1400 }}
        />
      ) : null}
    </ListPageLayout>
  );
}
