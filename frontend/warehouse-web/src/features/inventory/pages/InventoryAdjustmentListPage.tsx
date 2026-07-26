import { Alert, Empty, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useListPagination } from "../../../shared/pagination/pagination";
import { useInventoryAdjustments } from "../api/useInventory";
import type { InventoryAdjustmentListItem } from "../api/inventoryTypes";
import { inventoryRoutes } from "../inventoryConstants";
import {
  ListPageLayout,
  ReturnAwareLink,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";

export function InventoryAdjustmentListPage() {
  const { i18n, t } = useTranslation(); const pagination = useListPagination(); const adjustments = useInventoryAdjustments(pagination.page, pagination.pageSize);
  const columns: ColumnsType<InventoryAdjustmentListItem> = [{ title: t("inventory.table.reason"), dataIndex: "reason", render: (value) => t(`inventory.reasons.${value}`) }, { title: t("inventory.table.reference"), dataIndex: "reference", render: value => value ?? "—" }, { title: t("inventory.table.lines"), dataIndex: "lineCount" }, { title: t("inventory.table.created"), dataIndex: "createdAtUtc", render: value => formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)) }, { title: t("inventory.table.actions"), key: "actions", render: (_, item) => <ReturnAwareLink to={inventoryRoutes.adjustmentDetail(item.id)}>{t("inventory.view")}</ReturnAwareLink> }];
  return <ListPageLayout actions={<RouteActionButton to={inventoryRoutes.adjustmentCreate} type="primary">{t("inventory.newAdjustment")}</RouteActionButton>} subtitle={t("inventory.adjustmentsSubtitle")} title={t("inventory.adjustmentsTitle")}>{adjustments.isLoading ? <Spin className="page-spinner" size="large" tip={t("inventory.loadingAdjustments")} /> : adjustments.error ? <Alert message={getErrorMessage(t, adjustments.error, "inventory.errors.loadAdjustments")} showIcon type="error" /> : adjustments.data?.items.length === 0 ? <Empty description={t("inventory.emptyAdjustments")} /> : <Table columns={columns} dataSource={adjustments.data?.items} loading={adjustments.isFetching} pagination={adjustments.data ? pagination.toTablePagination(adjustments.data) : false} rowKey="id" />}</ListPageLayout>;
}
