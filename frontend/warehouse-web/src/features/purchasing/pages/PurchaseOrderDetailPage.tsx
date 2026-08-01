import {
  Alert,
  Button,
  Descriptions,
  Popconfirm,
  Spin,
  Table,
  Tag,
  Timeline,
  Typography,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import {
  useCancelPurchaseOrder,
  usePurchaseOrder,
  useSubmitPurchaseOrder,
} from "../api/usePurchasing";
import {
  purchaseOrderStatusColors,
  purchaseOrderStatusTranslationKeys,
  type PurchaseOrderLine,
} from "../api/purchasingTypes";
import { purchasingRoutes } from "../purchasingConstants";
import { receivingRoutes } from "../../receiving/receivingConstants";
import {
  DetailPageLayout,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function PurchaseOrderDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const orderQuery = usePurchaseOrder(id);
  const submit = useSubmitPurchaseOrder(id ?? "");
  const cancel = useCancelPurchaseOrder(id ?? "");
  const { returnTo } = useReturnDestination(purchasingRoutes.orders);
  if (orderQuery.isLoading)
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("purchasing.orders.loadingOne")}
      />
    );
  if (orderQuery.error || !orderQuery.data || !id)
    return (
      <Alert
        message={getErrorMessage(
          t,
          orderQuery.error,
          "purchasing.orders.errors.load",
        )}
        showIcon
        type="error"
      />
    );
  const order = orderQuery.data;
  const columns: ColumnsType<PurchaseOrderLine> = [
    {
      title: t("purchasing.orders.lineNumber"),
      dataIndex: "lineNumber",
      key: "lineNumber",
      width: 72,
    },
    {
      title: t("purchasing.orders.product"),
      key: "product",
      render: (_, line) => `${line.productSku} — ${line.productName}`,
    },
    {
      title: t("purchasing.orders.quantity"),
      key: "quantity",
      render: (_, line) => `${line.quantity} ${line.purchaseUnitOfMeasure}`,
    },
    {
      title: t("purchasing.orders.baseQuantity"),
      key: "baseQuantity",
      render: (_, line) => line.quantityInBaseUnit,
    },
    {
      title: t("purchasing.orders.unitPrice"),
      key: "unitPrice",
      render: (_, line) => `${line.unitPrice} ${line.currencyCode}`,
    },
    {
      title: t("purchasing.orders.lineTotal"),
      key: "lineTotal",
      render: (_, line) => `${line.lineAmount} ${line.currencyCode}`,
    },
  ];
  return (
    <DetailPageLayout
      actions={
        order.status === "Draft" ||
        order.status === "Submitted" ||
        order.status === "PartiallyReceived" ? (
          <>
            {order.status === "Draft" ? (
              <>
                <RouteActionButton to={purchasingRoutes.orderEdit(id)}>
                  {t("purchasing.edit")}
                </RouteActionButton>
                <Popconfirm
                  cancelText={t("purchasing.cancel")}
                  description={t("purchasing.orders.submitDescription")}
                  okText={t("purchasing.orders.submit")}
                  onConfirm={() => submit.mutateAsync(order.version)}
                  title={t("purchasing.orders.submitTitle")}
                >
                  <Button loading={submit.isPending} type="primary">
                    {t("purchasing.orders.submit")}
                  </Button>
                </Popconfirm>
              </>
            ) : null}
            {order.status === "Submitted" ||
            order.status === "PartiallyReceived" ? (
              <RouteActionButton
                to={receivingRoutes.create(order.id)}
                type="primary"
              >
                {t("receiving.record")}
              </RouteActionButton>
            ) : null}
            {order.status === "Draft" || order.status === "Submitted" ? (
              <Popconfirm
                cancelText={t("purchasing.cancel")}
                okText={t("purchasing.orders.cancelOrder")}
                onConfirm={() => cancel.mutateAsync({ version: order.version })}
                title={t("purchasing.orders.cancelOrder")}
              >
                <Button danger loading={cancel.isPending} type="text">
                  {t("purchasing.orders.cancelOrder")}
                </Button>
              </Popconfirm>
            ) : null}
          </>
        ) : undefined
      }
      backLabel={t("purchasing.orders.title")}
      backTo={returnTo}
      title={order.number ?? t("purchasing.orders.detailTitle")}
    >
      {submit.error || cancel.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            submit.error ?? cancel.error,
            submit.error
              ? "purchasing.orders.errors.submit"
              : "purchasing.orders.errors.cancel",
          )}
          showIcon
          type="error"
        />
      ) : null}
      <Descriptions bordered column={1}>
        <Descriptions.Item label={t("purchasing.orders.number")}>
          {order.number ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.supplier")}>
          {order.supplierCode} — {order.supplierName}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.warehouse")}>
          {order.destinationWarehouseCode
            ? `${order.destinationWarehouseCode} — ${order.destinationWarehouseName}`
            : "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.currency")}>
          {order.currencyCode ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.orderDate")}>
          {order.orderDate ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.buyer")}>
          {order.buyerUserId ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.expectedDeliveryDate")}>
          {order.expectedDeliveryDate ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.supplierReference")}>
          {order.supplierReference ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.notes")}>
          {order.notes ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.total")}>
          {`${order.totalAmount} ${order.currencyCode ?? ""}`}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.status")}>
          <Tag color={purchaseOrderStatusColors[order.status]}>
            {t(purchaseOrderStatusTranslationKeys[order.status])}
          </Tag>
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.created")}>
          {formatDateTime(
            order.createdAtUtc,
            toAppLanguage(i18n.resolvedLanguage),
          )}
        </Descriptions.Item>
        <Descriptions.Item label={t("purchasing.orders.updated")}>
          {formatDateTime(
            order.updatedAtUtc,
            toAppLanguage(i18n.resolvedLanguage),
          )}
        </Descriptions.Item>
      </Descriptions>
      <div className="page-section">
        <Typography.Title level={3}>
          {t("purchasing.orders.lines")}
        </Typography.Title>
        <Table
          columns={columns}
          dataSource={order.lines}
          pagination={false}
          rowKey="id"
        />
      </div>
      <div className="page-section">
        <Typography.Title level={3}>
          {t("purchasing.orders.status")}
        </Typography.Title>
        <Timeline
          items={order.statusHistory.map((history) => ({
            children: `${t(purchaseOrderStatusTranslationKeys[history.status])} — ${formatDateTime(history.changedAtUtc, toAppLanguage(i18n.resolvedLanguage))}`,
          }))}
        />
      </div>
    </DetailPageLayout>
  );
}
