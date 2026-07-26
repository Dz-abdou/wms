import { Alert, Card, Descriptions, Spin, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useInventoryAdjustment } from "../api/useInventory";
import type { InventoryAdjustmentLine } from "../api/inventoryTypes";

export function InventoryAdjustmentDetailPage() {
  const { id } = useParams(); const { i18n, t } = useTranslation(); const adjustment = useInventoryAdjustment(id);
  if (adjustment.isLoading) return <Spin className="page-spinner" size="large" tip={t("inventory.loadingAdjustment")} />;
  if (adjustment.error || !adjustment.data) return <Alert message={getErrorMessage(t, adjustment.error, "inventory.errors.loadAdjustment")} showIcon type="error" />;
  const item = adjustment.data; const columns: ColumnsType<InventoryAdjustmentLine> = [{ title: t("inventory.table.product"), key: "product", render: (_, x) => `${x.productSku} — ${x.productName}` }, { title: t("inventory.table.warehouse"), key: "warehouse", render: (_, x) => `${x.warehouseCode} — ${x.warehouseName}` }, { title: t("inventory.table.type"), dataIndex: "type", render: value => t(value === "ManualIncrease" ? "inventory.types.increase" : "inventory.types.decrease") }, { title: t("inventory.table.delta"), key: "delta", render: (_, x) => `${x.quantityDeltaInUnit} ${x.unitOfMeasure}` }, { title: t("inventory.table.balanceAfter"), dataIndex: "balanceAfter" }];
  return <section><Typography.Title level={2}>{t("inventory.adjustmentDetailTitle")}</Typography.Title><Card><Descriptions column={1} items={[{ key: "reason", label: t("inventory.form.reason"), children: t(`inventory.reasons.${item.reason}`) }, { key: "reference", label: t("inventory.form.reference"), children: item.reference ?? "—" }, { key: "note", label: t("inventory.form.note"), children: item.note ?? "—" }, { key: "created", label: t("inventory.table.created"), children: formatDateTime(item.createdAtUtc, toAppLanguage(i18n.resolvedLanguage)) }]} /></Card><Card className="inventory-history-card" title={t("inventory.lines")}><Table columns={columns} dataSource={item.lines} pagination={false} rowKey="movementId" /></Card></section>;
}
