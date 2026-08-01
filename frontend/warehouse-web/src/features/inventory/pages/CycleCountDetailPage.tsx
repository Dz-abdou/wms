import { Alert, Descriptions, Spin, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import { DetailPageLayout } from "../../../shared/components/PageLayouts";
import { QuantityDelta } from "../../../shared/components/QuantityDelta";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useCycleCount } from "../api/useInventory";
import type { CycleCountLine } from "../api/inventoryTypes";
import { inventoryRoutes } from "../inventoryConstants";

export function CycleCountDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const cycleCount = useCycleCount(id);
  const { returnTo } = useReturnDestination(inventoryRoutes.cycleCounts);

  if (cycleCount.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("inventory.cycleCounts.loadingOne")}
      />
    );
  }

  if (cycleCount.error || !cycleCount.data) {
    return (
      <Alert
        message={getErrorMessage(
          t,
          cycleCount.error,
          "inventory.errors.loadCycleCount",
        )}
        showIcon
        type="error"
      />
    );
  }

  const item = cycleCount.data;
  const columns: ColumnsType<CycleCountLine> = [
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
      title: t("inventory.cycleCounts.systemQuantity"),
      key: "systemQuantity",
      render: (_, line) =>
        `${line.systemQuantityInBase} ${line.baseUnitOfMeasure}`,
    },
    {
      title: t("inventory.cycleCounts.countedQuantity"),
      key: "countedQuantity",
      render: (_, line) =>
        `${line.countedQuantityInUnit} ${line.countedUnitOfMeasure}`,
    },
    {
      title: t("inventory.cycleCounts.variance"),
      dataIndex: "varianceQuantityInBase",
      render: (value) => <QuantityDelta value={value} />,
    },
    { title: t("inventory.cycleCounts.unit"), dataIndex: "baseUnitOfMeasure" },
  ];

  return (
    <DetailPageLayout
      backLabel={t("inventory.cycleCounts.title")}
      backTo={returnTo}
      title={t("inventory.cycleCounts.detailTitle")}
    >
      <Descriptions
        bordered
        column={1}
        items={[
          {
            key: "warehouse",
            label: t("inventory.table.warehouse"),
            children: `${item.warehouseCode} — ${item.warehouseName}`,
          },
          {
            key: "number",
            label: t("inventory.table.number"),
            children: item.number,
          },
          {
            key: "reference",
            label: t("inventory.table.externalReference"),
            children: item.reference ?? "—",
          },
          {
            key: "note",
            label: t("inventory.form.note"),
            children: item.note ?? "—",
          },
          {
            key: "countedAt",
            label: t("inventory.cycleCounts.countedAt"),
            children: formatDateTime(
              item.countedAtUtc,
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
          rowKey="id"
          scroll={{ x: 1050 }}
        />
      </div>
    </DetailPageLayout>
  );
}
