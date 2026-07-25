import { Alert, Button, Empty, Spin, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useListPagination } from "../../../shared/pagination/pagination";
import { useInventoryAdjustments } from "../api/useInventory";
import type { InventoryAdjustmentListItem } from "../api/inventoryTypes";
import { inventoryRoutes } from "../inventoryConstants";

export function InventoryAdjustmentListPage() {
  const { i18n, t } = useTranslation(); const navigate = useNavigate(); const pagination = useListPagination(); const adjustments = useInventoryAdjustments(pagination.page, pagination.pageSize);
  const columns: ColumnsType<InventoryAdjustmentListItem> = [{ title: t("inventory.table.reason"), dataIndex: "reason", render: (value) => t(`inventory.reasons.${value}`) }, { title: t("inventory.table.reference"), dataIndex: "reference", render: value => value ?? "—" }, { title: t("inventory.table.lines"), dataIndex: "lineCount" }, { title: t("inventory.table.created"), dataIndex: "createdAtUtc", render: value => formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)) }, { title: t("inventory.table.actions"), key: "actions", render: (_, item) => <Link to={inventoryRoutes.adjustmentDetail(item.id)}>{t("inventory.view")}</Link> }];
  return <section><div className="page-heading"><div><Typography.Title level={2}>{t("inventory.adjustmentsTitle")}</Typography.Title><Typography.Paragraph>{t("inventory.adjustmentsSubtitle")}</Typography.Paragraph></div><Button onClick={() => navigate(inventoryRoutes.adjustmentCreate)} type="primary">{t("inventory.newAdjustment")}</Button></div>{adjustments.isLoading ? <Spin className="page-spinner" size="large" tip={t("inventory.loadingAdjustments")} /> : adjustments.error ? <Alert message={getErrorMessage(t, adjustments.error, "inventory.errors.loadAdjustments")} showIcon type="error" /> : adjustments.data?.items.length === 0 ? <Empty description={t("inventory.emptyAdjustments")} /> : <Table columns={columns} dataSource={adjustments.data?.items} loading={adjustments.isFetching} pagination={adjustments.data ? pagination.toTablePagination(adjustments.data) : false} rowKey="id" />}</section>;
}
