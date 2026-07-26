import { Alert, Empty, Spin, Table, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useListPagination } from "../../../shared/pagination/pagination";
import { usePurchaseOrders } from "../api/usePurchasing";
import type { PurchaseOrder } from "../api/purchasingTypes";
import { purchasingRoutes } from "../purchasingConstants";
import {
  ListPageLayout,
  ReturnAwareLink,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";

export function PurchaseOrderListPage() {
  const { t } = useTranslation(); const pagination = useListPagination(); const orders = usePurchaseOrders(pagination.page, pagination.pageSize);
  const columns = useMemo<ColumnsType<PurchaseOrder>>(() => [
    { title: t("purchasing.orders.supplier"), key: "supplier", render: (_, item) => `${item.supplierCode} — ${item.supplierName}` },
    { title: t("purchasing.orders.status"), key: "status", render: (_, item) => <Tag color={item.status === 0 ? "gold" : "green"}>{t(item.status === 0 ? "purchasing.status.draft" : "purchasing.status.submitted")}</Tag> },
    { title: t("purchasing.orders.actions"), key: "actions", render: (_, item) => <ReturnAwareLink to={purchasingRoutes.orderDetail(item.id)}>{t("purchasing.view")}</ReturnAwareLink> },
  ], [t]);
  return <ListPageLayout actions={<RouteActionButton to={purchasingRoutes.orderCreate} type="primary">{t("purchasing.orders.new")}</RouteActionButton>} subtitle={t("purchasing.orders.subtitle")} title={t("purchasing.orders.title")}>{orders.isLoading ? <Spin className="page-spinner" size="large" tip={t("purchasing.orders.loading")} /> : null}{orders.error ? <Alert className="page-alert" message={getErrorMessage(t, orders.error, "purchasing.orders.errors.load")} showIcon type="error" /> : null}{orders.data && orders.data.items.length === 0 ? <Empty description={t("purchasing.orders.empty")} /> : null}{orders.data && orders.data.items.length > 0 ? <Table columns={columns} dataSource={orders.data.items} loading={orders.isFetching} pagination={pagination.toTablePagination(orders.data)} rowKey="id" /> : null}</ListPageLayout>;
}
