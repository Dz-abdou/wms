import { Alert, Button, Card, Spin } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  getErrorMessage,
  hasProblemCode,
} from "../../../shared/errors/problemDetails";
import { usePurchaseOrder, useUpdatePurchaseOrder } from "../api/usePurchasing";
import type { PurchaseOrderInput } from "../api/purchasingTypes";
import { PurchaseOrderForm } from "../components/PurchaseOrderForm";
import { purchasingRoutes } from "../purchasingConstants";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function PurchaseOrderEditPage() {
  const { id } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const orderQuery = usePurchaseOrder(id);
  const update = useUpdatePurchaseOrder(id ?? "");
  const { goBack, returnTo } = useReturnDestination(
    purchasingRoutes.orderDetail(id ?? ""),
  );
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
  if (order.status !== "Draft")
    return (
      <Alert
        message={t("purchasing.orders.submittedReadOnly")}
        showIcon
        type="warning"
      />
    );
  async function submit(values: PurchaseOrderInput) {
    await update.mutateAsync(values);
    navigate(purchasingRoutes.orderDetail(id!));
  }
  return (
    <FormPageLayout
      backLabel={t("purchasing.orders.detailTitle")}
      backTo={returnTo}
      title={t("purchasing.orders.editTitle")}
    >
      {update.error ? (
        <Alert
          action={
            hasProblemCode(
              update.error,
              "purchase_order.concurrency_conflict",
            ) ? (
              <Button
                onClick={async () => {
                  await orderQuery.refetch();
                  update.reset();
                }}
              >
                {t("purchasing.orders.refresh")}
              </Button>
            ) : undefined
          }
          className="page-alert"
          description={
            hasProblemCode(update.error, "purchase_order.concurrency_conflict")
              ? t("purchasing.orders.concurrencyHint")
              : undefined
          }
          message={getErrorMessage(
            t,
            update.error,
            "purchasing.orders.errors.update",
          )}
          showIcon
          type="error"
        />
      ) : null}
      <Card>
        <PurchaseOrderForm
          key={order.version}
          cancelLabel={t("purchasing.cancel")}
          errorMessageKey="purchasing.orders.errors.update"
          initialValues={{
            supplierId: order.supplierId,
            destinationWarehouseId: order.destinationWarehouseId,
            currencyCode: order.currencyCode,
            orderDate: order.orderDate,
            expectedDeliveryDate: order.expectedDeliveryDate,
            supplierReference: order.supplierReference,
            notes: order.notes,
            version: order.version,
            lines: order.lines.map((line) => ({
              supplierProductId: line.supplierProductId,
              quantity: line.quantity,
            })),
          }}
          isSubmitting={update.isPending}
          onCancel={goBack}
          onSubmit={submit}
          submitLabel={t("purchasing.save")}
        />
      </Card>
    </FormPageLayout>
  );
}
