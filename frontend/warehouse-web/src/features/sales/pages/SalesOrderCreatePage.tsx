import { Alert, Card } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useCreateSalesOrder } from "../api/useSalesOrders";
import type { SalesOrderInput } from "../api/salesTypes";
import { SalesOrderForm } from "../components/SalesOrderForm";
import { salesRoutes } from "../salesConstants";

export function SalesOrderCreatePage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const create = useCreateSalesOrder();
  const { goBack, returnTo } = useReturnDestination(salesRoutes.orders);
  async function submit(values: SalesOrderInput) {
    const order = await create.mutateAsync(values);
    navigate(salesRoutes.detail(order.id));
  }
  return (
    <FormPageLayout
      backLabel={t("sales.orders.title")}
      backTo={returnTo}
      title={t("sales.orders.createTitle")}
    >
      {create.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            create.error,
            "sales.orders.errors.create",
          )}
          showIcon
          type="error"
        />
      ) : null}
      <Card>
        <SalesOrderForm
          cancelLabel={t("ui.cancel")}
          errorMessageKey="sales.orders.errors.create"
          isSubmitting={create.isPending}
          onCancel={goBack}
          onSubmit={submit}
          submitLabel={t("sales.orders.create")}
        />
      </Card>
    </FormPageLayout>
  );
}
