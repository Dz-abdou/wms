import { Alert, Descriptions, Spin, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import { DetailPageLayout } from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useInventoryAdjustment } from "../api/useInventory";
import type { InventoryAdjustmentLine } from "../api/inventoryTypes";
import { inventoryRoutes } from "../inventoryConstants";

export function InventoryAdjustmentDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const adjustment = useInventoryAdjustment(id);
  const { returnTo } = useReturnDestination(inventoryRoutes.adjustments);

  if (adjustment.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("inventory.loadingAdjustment")}
      />
    );
  }

  if (adjustment.error || !adjustment.data) {
    return (
      <Alert
        message={getErrorMessage(
          t,
          adjustment.error,
          "inventory.errors.loadAdjustment",
        )}
        showIcon
        type="error"
      />
    );
  }

  const item = adjustment.data;
  const columns: ColumnsType<InventoryAdjustmentLine> = [
    {
      title: t("inventory.table.lineNumber"),
      dataIndex: "lineNumber",
      key: "lineNumber",
      width: 72,
    },
    {
      title: t("inventory.table.product"),
      key: "product",
      render: (_, line) => `${line.productSku} — ${line.productName}`,
    },
    {
      title: t("inventory.table.warehouse"),
      key: "warehouse",
      render: (_, line) => `${line.warehouseCode} — ${line.warehouseName}`,
    },
    {
      title: t("inventory.table.type"),
      dataIndex: "type",
      key: "type",
      render: (value) =>
        t(
          value === "ManualIncrease"
            ? "inventory.types.increase"
            : "inventory.types.decrease",
        ),
    },
    {
      title: t("inventory.table.delta"),
      key: "delta",
      render: (_, line) => `${line.quantityDeltaInUnit} ${line.unitOfMeasure}`,
    },
    {
      title: t("inventory.table.balanceAfter"),
      dataIndex: "balanceAfter",
      key: "balanceAfter",
    },
  ];

  return (
    <DetailPageLayout
      backLabel={t("inventory.adjustmentsTitle")}
      backTo={returnTo}
      title={t("inventory.adjustmentDetailTitle")}
    >
      <Descriptions
        bordered
        column={1}
        items={[
          {
            key: "reason",
            label: t("inventory.form.reason"),
            children: t(`inventory.reasons.${item.reason}`),
          },
          {
            key: "number",
            label: t("inventory.table.number"),
            children: item.number,
          },
          {
            key: "reference",
            label: t("inventory.form.reference"),
            children: item.reference ?? "—",
          },
          {
            key: "note",
            label: t("inventory.form.note"),
            children: item.note ?? "—",
          },
          {
            key: "created",
            label: t("inventory.table.created"),
            children: formatDateTime(
              item.createdAtUtc,
              toAppLanguage(i18n.resolvedLanguage),
            ),
          },
        ]}
      />
      <div className="page-section">
        <Typography.Title level={3}>{t("inventory.lines")}</Typography.Title>
        <Table
          columns={columns}
          dataSource={item.lines}
          pagination={false}
          rowKey="movementId"
          scroll={{ x: 1050 }}
        />
      </div>
    </DetailPageLayout>
  );
}
