import { Alert, Descriptions, Spin, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  DetailPageLayout,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { purchasingRoutes } from "../../purchasing/purchasingConstants";
import { useGoodsReceipt } from "../api/useReceiving";
import type { GoodsReceiptLine } from "../api/receivingTypes";
import { receivingRoutes } from "../receivingConstants";

export function GoodsReceiptDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const receiptQuery = useGoodsReceipt(id);
  const { returnTo } = useReturnDestination(receivingRoutes.list);
  if (receiptQuery.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("receiving.loadingOne")}
      />
    );
  }
  if (receiptQuery.error || !receiptQuery.data) {
    return (
      <Alert
        message={getErrorMessage(
          t,
          receiptQuery.error,
          "receiving.errors.load",
        )}
        showIcon
        type="error"
      />
    );
  }

  const receipt = receiptQuery.data;
  const columns: ColumnsType<GoodsReceiptLine> = [
    {
      title: t("receiving.lineNumber"),
      dataIndex: "purchaseOrderLineNumber",
      key: "purchaseOrderLineNumber",
      width: 80,
    },
    {
      title: t("receiving.product"),
      key: "product",
      render: (_, line) => `${line.productSku} — ${line.productName}`,
    },
    {
      title: t("receiving.unit"),
      dataIndex: "unitOfMeasure",
      key: "unitOfMeasure",
    },
    {
      title: t("receiving.acceptedQuantity"),
      key: "acceptedQuantity",
      render: (_, line) => `${line.acceptedQuantity} ${line.unitOfMeasure}`,
    },
    {
      title: t("receiving.baseQuantity"),
      dataIndex: "acceptedQuantityInBaseUnit",
      key: "acceptedQuantityInBaseUnit",
    },
  ];

  return (
    <DetailPageLayout
      backLabel={t("receiving.title")}
      backTo={returnTo}
      title={receipt.number}
    >
      <Descriptions bordered column={1}>
        <Descriptions.Item label={t("receiving.purchaseOrder")}>
          <ReturnAwareLink
            to={purchasingRoutes.orderDetail(receipt.purchaseOrderId)}
          >
            {receipt.purchaseOrderNumber}
          </ReturnAwareLink>
        </Descriptions.Item>
        <Descriptions.Item label={t("receiving.warehouse")}>
          {receipt.warehouseCode} — {receipt.warehouseName}
        </Descriptions.Item>
        <Descriptions.Item label={t("receiving.receivedAt")}>
          {formatDateTime(
            receipt.receivedAtUtc,
            toAppLanguage(i18n.resolvedLanguage),
          )}
        </Descriptions.Item>
        <Descriptions.Item label={t("receiving.supplierDeliveryNote")}>
          {receipt.supplierDeliveryNote ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("receiving.notes")}>
          {receipt.notes ?? "—"}
        </Descriptions.Item>
      </Descriptions>
      <div className="page-section">
        <Typography.Title level={3}>{t("receiving.lines")}</Typography.Title>
        <Table
          columns={columns}
          dataSource={receipt.lines}
          pagination={false}
          rowKey="id"
          scroll={{ x: 850 }}
        />
      </div>
    </DetailPageLayout>
  );
}
