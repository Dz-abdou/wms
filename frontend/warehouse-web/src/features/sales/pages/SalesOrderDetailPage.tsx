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
import {
  DetailPageLayout,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import {
  useCancelSalesOrder,
  useSalesOrder,
  useSubmitSalesOrder,
} from "../api/useSalesOrders";
import {
  salesOrderStatusColors,
  salesOrderStatusTranslationKeys,
  type SalesOrderLine,
} from "../api/salesTypes";
import { salesRoutes } from "../salesConstants";

export function SalesOrderDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const orderQuery = useSalesOrder(id);
  const submit = useSubmitSalesOrder(id ?? "");
  const cancel = useCancelSalesOrder(id ?? "");
  const { returnTo } = useReturnDestination(salesRoutes.orders);
  if (orderQuery.isLoading)
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("sales.orders.loadingOne")}
      />
    );
  if (orderQuery.error || !orderQuery.data || !id)
    return (
      <Alert
        message={getErrorMessage(
          t,
          orderQuery.error,
          "sales.orders.errors.load",
        )}
        showIcon
        type="error"
      />
    );
  const order = orderQuery.data;
  const lines: ColumnsType<SalesOrderLine> = [
    {
      title: t("sales.orders.lineNumber"),
      dataIndex: "lineNumber",
      key: "lineNumber",
      width: 72,
    },
    {
      title: t("sales.orders.product"),
      key: "product",
      render: (_, line) => `${line.productSku} — ${line.productName}`,
    },
    {
      title: t("sales.orders.unit"),
      dataIndex: "unitOfMeasure",
      key: "unitOfMeasure",
    },
    {
      title: t("sales.orders.quantity"),
      dataIndex: "quantity",
      key: "quantity",
    },
    {
      title: t("sales.orders.baseQuantity"),
      dataIndex: "quantityInBaseUnit",
      key: "quantityInBaseUnit",
    },
  ];
  const address = order.shippingAddress;
  return (
    <DetailPageLayout
      actions={
        order.status === "Draft" ? (
          <>
            <RouteActionButton to={salesRoutes.edit(id)}>
              {t("sales.edit")}
            </RouteActionButton>
            <Popconfirm
              cancelText={t("ui.cancel")}
              description={t("sales.orders.submitDescription")}
              okText={t("sales.orders.submit")}
              onConfirm={() => submit.mutateAsync(order.version)}
              title={t("sales.orders.submitTitle")}
            >
              <Button loading={submit.isPending} type="primary">
                {t("sales.orders.submit")}
              </Button>
            </Popconfirm>
            <Popconfirm
              cancelText={t("ui.cancel")}
              okText={t("sales.orders.cancelOrder")}
              onConfirm={() => cancel.mutateAsync({ version: order.version })}
              title={t("sales.orders.cancelOrder")}
            >
              <Button danger loading={cancel.isPending} type="text">
                {t("sales.orders.cancelOrder")}
              </Button>
            </Popconfirm>
          </>
        ) : undefined
      }
      backLabel={t("sales.orders.title")}
      backTo={returnTo}
      title={order.number}
    >
      {submit.error || cancel.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            submit.error ?? cancel.error,
            submit.error
              ? "sales.orders.errors.submit"
              : "sales.orders.errors.cancel",
          )}
          showIcon
          type="error"
        />
      ) : null}
      <Descriptions bordered column={1}>
        <Descriptions.Item label={t("sales.orders.number")}>
          {order.number}
        </Descriptions.Item>
        <Descriptions.Item label={t("sales.orders.customer")}>
          {order.customerCode} — {order.customerName}
        </Descriptions.Item>
        <Descriptions.Item label={t("sales.orders.shippingAddress")}>
          {address.label} — {address.addressLine1}, {address.city},{" "}
          {address.countryCode}
        </Descriptions.Item>
        <Descriptions.Item label={t("sales.orders.currency")}>
          {order.currencyCode}
        </Descriptions.Item>
        <Descriptions.Item label={t("sales.orders.orderDate")}>
          {order.orderDate}
        </Descriptions.Item>
        <Descriptions.Item label={t("sales.orders.requestedShipDate")}>
          {order.requestedShipDate ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("sales.orders.customerReference")}>
          {order.customerReference ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("sales.orders.deliveryInstructions")}>
          {order.deliveryInstructions ?? address.deliveryInstructions ?? "—"}
        </Descriptions.Item>
        <Descriptions.Item label={t("sales.orders.status")}>
          <Tag color={salesOrderStatusColors[order.status]}>
            {t(salesOrderStatusTranslationKeys[order.status])}
          </Tag>
        </Descriptions.Item>
      </Descriptions>
      <div className="page-section">
        <Typography.Title level={3}>{t("sales.orders.lines")}</Typography.Title>
        <Table
          columns={lines}
          dataSource={order.lines}
          pagination={false}
          rowKey="id"
          scroll={{ x: 800 }}
        />
      </div>
      <div className="page-section">
        <Typography.Title level={3}>
          {t("sales.orders.status")}
        </Typography.Title>
        <Timeline
          items={order.statusHistory.map((history) => ({
            children: `${t(salesOrderStatusTranslationKeys[history.status])} — ${formatDateTime(history.changedAtUtc, toAppLanguage(i18n.resolvedLanguage))}`,
          }))}
        />
      </div>
    </DetailPageLayout>
  );
}
