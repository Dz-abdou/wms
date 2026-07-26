import { Alert, Card } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useCreatePurchaseOrder } from "../api/usePurchasing";
import type { PurchaseOrderInput } from "../api/purchasingTypes";
import { PurchaseOrderForm } from "../components/PurchaseOrderForm";
import { purchasingRoutes } from "../purchasingConstants";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function PurchaseOrderCreatePage() {
  const { t } = useTranslation(); const navigate = useNavigate(); const create = useCreatePurchaseOrder();
  const { goBack, returnTo } = useReturnDestination(purchasingRoutes.orders);
  async function submit(values: PurchaseOrderInput) { const order = await create.mutateAsync(values); navigate(purchasingRoutes.orderDetail(order.id)); }
  return <FormPageLayout backLabel={t("purchasing.orders.title")} backTo={returnTo} title={t("purchasing.orders.createTitle")}>{create.error ? <Alert className="page-alert" message={getErrorMessage(t, create.error, "purchasing.orders.errors.create")} showIcon type="error" /> : null}<Card><PurchaseOrderForm cancelLabel={t("purchasing.cancel")} errorMessageKey="purchasing.orders.errors.create" isSubmitting={create.isPending} onCancel={goBack} onSubmit={submit} submitLabel={t("purchasing.orders.create")} /></Card></FormPageLayout>;
}
