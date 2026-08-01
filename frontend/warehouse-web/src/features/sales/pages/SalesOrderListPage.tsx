import { Alert, Empty, Input, Select, Spin, Table, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import {
  ListFilter,
  ListPageLayout,
  NewPageAction,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useCustomers } from "../../customers/api/useCustomers";
import { useSalesOrders } from "../api/useSalesOrders";
import {
  salesOrderStatusColors,
  salesOrderStatusTranslationKeys,
  type SalesOrder,
} from "../api/salesTypes";
import { salesRoutes } from "../salesConstants";

export function SalesOrderListPage() {
  const { t } = useTranslation();
  const listQuery = useUrlListQuery();
  const [customerSearch, setCustomerSearch] = useState("");
  const status = listQuery.get("status");
  const orders = useSalesOrders({
    ...listQuery.request,
    customerId: listQuery.get("customerId"),
    status: status as SalesOrder["status"] | undefined,
    fromOrderDate: listQuery.get("fromOrderDate"),
    toOrderDate: listQuery.get("toOrderDate"),
  });
  const customers = useCustomers({
    page: 1,
    pageSize: 20,
    search: customerSearch,
  });
  const columns = useMemo<ColumnsType<SalesOrder>>(
    () => [
      { title: t("sales.orders.number"), dataIndex: "number", key: "number" },
      {
        title: t("sales.orders.customer"),
        key: "customer",
        render: (_, order) => `${order.customerCode} — ${order.customerName}`,
      },
      {
        title: t("sales.orders.orderDate"),
        dataIndex: "orderDate",
        key: "orderDate",
      },
      {
        title: t("sales.orders.requestedShipDate"),
        dataIndex: "requestedShipDate",
        key: "requestedShipDate",
        render: (value: string | null) => value ?? "—",
      },
      {
        title: t("sales.orders.lineCount"),
        key: "lineCount",
        render: (_, order) => order.lines.length,
      },
      {
        title: t("sales.orders.currency"),
        dataIndex: "currencyCode",
        key: "currencyCode",
      },
      {
        title: t("sales.orders.status"),
        key: "status",
        render: (_, order) => (
          <Tag color={salesOrderStatusColors[order.status]}>
            {t(salesOrderStatusTranslationKeys[order.status])}
          </Tag>
        ),
      },
      {
        title: t("sales.orders.actions"),
        key: "actions",
        fixed: "right",
        width: 120,
        render: (_, order) => (
          <ReturnAwareLink to={salesRoutes.detail(order.id)}>
            {t("sales.view")}
          </ReturnAwareLink>
        ),
      },
    ],
    [t],
  );
  return (
    <ListPageLayout
      actions={<NewPageAction to={salesRoutes.create} />}
      filters={
        <>
          <ListFilter label={t("sales.orders.customer")} width="regular">
            <Select
              allowClear
              aria-label={t("sales.orders.customer")}
              filterOption={false}
              onChange={(value) => listQuery.update({ customerId: value })}
              onSearch={setCustomerSearch}
              options={(customers.data?.items ?? []).map((customer) => ({
                value: customer.id,
                label: `${customer.code} — ${customer.tradingName ?? customer.legalName}`,
              }))}
              placeholder={t("sales.orders.customer")}
              showSearch
              value={listQuery.get("customerId")}
            />
          </ListFilter>
          <ListFilter label={t("sales.orders.status")} width="compact">
            <Select
              allowClear
              aria-label={t("sales.orders.status")}
              onChange={(value) => listQuery.update({ status: value })}
              options={[
                { value: "Draft", label: t("sales.status.draft") },
                { value: "Submitted", label: t("sales.status.submitted") },
                { value: "Cancelled", label: t("sales.status.cancelled") },
              ]}
              placeholder={t("sales.orders.status")}
              value={status}
            />
          </ListFilter>
          <ListFilter label={t("sales.orders.fromDate")} width="compact">
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
          <ListFilter label={t("sales.orders.toDate")} width="compact">
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
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      subtitle={t("sales.orders.subtitle")}
      title={t("sales.orders.title")}
    >
      {orders.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("sales.orders.loading")}
        />
      ) : null}
      {orders.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, orders.error, "sales.orders.errors.load")}
          showIcon
          type="error"
        />
      ) : null}
      {orders.data && orders.data.items.length === 0 ? (
        <Empty description={t("sales.orders.empty")} />
      ) : null}
      {orders.data && orders.data.items.length > 0 ? (
        <Table
          columns={columns}
          dataSource={orders.data.items}
          loading={orders.isFetching}
          pagination={listQuery.toTablePagination(orders.data)}
          rowKey="id"
          scroll={{ x: 1200 }}
        />
      ) : null}
    </ListPageLayout>
  );
}
