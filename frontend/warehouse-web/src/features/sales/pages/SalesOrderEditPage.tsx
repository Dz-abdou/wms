import { Alert, Button, Card, Spin } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import {
  getErrorMessage,
  hasProblemCode,
} from "../../../shared/errors/problemDetails";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useSalesOrder, useUpdateSalesOrder } from "../api/useSalesOrders";
import type { SalesOrderInput } from "../api/salesTypes";
import { SalesOrderForm } from "../components/SalesOrderForm";
import { salesRoutes } from "../salesConstants";

export function SalesOrderEditPage() {
  const { id } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const orderQuery = useSalesOrder(id);
  const update = useUpdateSalesOrder(id ?? "");
  const { goBack, returnTo } = useReturnDestination(
    salesRoutes.detail(id ?? ""),
  );
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
  if (order.status !== "Draft")
    return (
      <Alert
        message={t("sales.orders.submittedReadOnly")}
        showIcon
        type="warning"
      />
    );
  async function submit(values: SalesOrderInput) {
    await update.mutateAsync(values);
    navigate(salesRoutes.detail(id!));
  }
  return (
    <FormPageLayout
      backLabel={t("sales.orders.detailTitle")}
      backTo={returnTo}
      title={t("sales.orders.editTitle")}
    >
      {update.error ? (
        <Alert
          action={
            hasProblemCode(update.error, "sales_order.concurrency_conflict") ? (
              <Button
                onClick={async () => {
                  await orderQuery.refetch();
                  update.reset();
                }}
              >
                {t("sales.orders.refresh")}
              </Button>
            ) : undefined
          }
          className="page-alert"
          description={
            hasProblemCode(update.error, "sales_order.concurrency_conflict")
              ? t("sales.orders.concurrencyHint")
              : undefined
          }
          message={getErrorMessage(
            t,
            update.error,
            "sales.orders.errors.update",
          )}
          showIcon
          type="error"
        />
      ) : null}
      <Card>
        <SalesOrderForm
          key={order.version}
          cancelLabel={t("ui.cancel")}
          errorMessageKey="sales.orders.errors.update"
          initialValues={{
            customerId: order.customerId,
            shippingAddressId: order.shippingAddressId,
            currencyCode: order.currencyCode,
            orderDate: order.orderDate,
            requestedShipDate: order.requestedShipDate ?? undefined,
            customerReference: order.customerReference ?? undefined,
            deliveryInstructions: order.deliveryInstructions ?? undefined,
            version: order.version,
            lines: order.lines.map(
              ({ productId, unitOfMeasure, quantity }) => ({
                productId,
                unitOfMeasure,
                quantity,
              }),
            ),
          }}
          isSubmitting={update.isPending}
          onCancel={goBack}
          onSubmit={submit}
          submitLabel={t("ui.save")}
        />
      </Card>
    </FormPageLayout>
  );
}
