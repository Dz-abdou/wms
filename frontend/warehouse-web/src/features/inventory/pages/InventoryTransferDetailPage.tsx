import { Alert, Descriptions, Spin, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import { DetailPageLayout } from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useTransfer } from "../api/useInventory";
import type { InventoryTransferLine } from "../api/inventoryTypes";
import { inventoryRoutes } from "../inventoryConstants";

export function InventoryTransferDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const transfer = useTransfer(id);
  const { returnTo } = useReturnDestination(inventoryRoutes.transfers);

  if (transfer.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("inventory.transfers.loadingOne")}
      />
    );
  }

  if (transfer.error || !transfer.data) {
    return (
      <Alert
        message={getErrorMessage(
          t,
          transfer.error,
          "inventory.errors.loadTransfer",
        )}
        showIcon
        type="error"
      />
    );
  }

  const item = transfer.data;
  const columns: ColumnsType<InventoryTransferLine> = [
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
      title: t("inventory.transfers.quantity"),
      key: "quantity",
      render: (_, line) => `${line.quantityInUnit} ${line.unitOfMeasure}`,
    },
    {
      title: t("inventory.transfers.baseQuantity"),
      dataIndex: "quantityInBaseUnit",
    },
    {
      title: t("inventory.transfers.sourceBalanceAfter"),
      dataIndex: "sourceBalanceAfter",
    },
    {
      title: t("inventory.transfers.destinationBalanceAfter"),
      dataIndex: "destinationBalanceAfter",
    },
  ];

  return (
    <DetailPageLayout
      backLabel={t("inventory.transfers.title")}
      backTo={returnTo}
      title={t("inventory.transfers.detailTitle")}
    >
      <Descriptions
        bordered
        column={1}
        items={[
          {
            key: "source",
            label: t("inventory.transfers.sourceWarehouse"),
            children: `${item.sourceWarehouseCode} — ${item.sourceWarehouseName}`,
          },
          {
            key: "destination",
            label: t("inventory.transfers.destinationWarehouse"),
            children: `${item.destinationWarehouseCode} — ${item.destinationWarehouseName}`,
          },
          {
            key: "reference",
            label: t("inventory.table.reference"),
            children: item.reference ?? "—",
          },
          {
            key: "note",
            label: t("inventory.form.note"),
            children: item.note ?? "—",
          },
          {
            key: "transferredAt",
            label: t("inventory.transfers.transferredAt"),
            children: formatDateTime(
              item.transferredAtUtc,
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
          scroll={{ x: 960 }}
        />
      </div>
    </DetailPageLayout>
  );
}
