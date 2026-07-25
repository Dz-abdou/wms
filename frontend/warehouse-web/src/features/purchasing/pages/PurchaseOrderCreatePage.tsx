import { Alert, Card, Typography } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useCreatePurchaseOrder } from "../api/usePurchasing";
import type { PurchaseOrderInput } from "../api/purchasingTypes";
import { PurchaseOrderForm } from "../components/PurchaseOrderForm";
import { purchasingRoutes } from "../purchasingConstants";

export function PurchaseOrderCreatePage() {
  const { t } = useTranslation(); const navigate = useNavigate(); const create = useCreatePurchaseOrder();
  async function submit(values: PurchaseOrderInput) { const order = await create.mutateAsync(values); navigate(purchasingRoutes.orderDetail(order.id)); }
  return <section><Typography.Title level={2}>{t("purchasing.orders.createTitle")}</Typography.Title>{create.error ? <Alert className="page-alert" message={getErrorMessage(t, create.error, "purchasing.orders.errors.create")} showIcon type="error" /> : null}<Card><PurchaseOrderForm errorMessageKey="purchasing.orders.errors.create" isSubmitting={create.isPending} onSubmit={submit} submitLabel={t("purchasing.orders.create")} /></Card></section>;
}
