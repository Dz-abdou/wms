import { Alert, Card, Spin, Typography } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { usePurchaseOrder, useUpdatePurchaseOrder } from "../api/usePurchasing";
import type { PurchaseOrderInput } from "../api/purchasingTypes";
import { PurchaseOrderForm } from "../components/PurchaseOrderForm";
import { purchasingRoutes } from "../purchasingConstants";

export function PurchaseOrderEditPage() {
  const { id } = useParams(); const { t } = useTranslation(); const navigate = useNavigate(); const orderQuery = usePurchaseOrder(id); const update = useUpdatePurchaseOrder(id ?? "");
  if (orderQuery.isLoading) return <Spin className="page-spinner" size="large" tip={t("purchasing.orders.loadingOne")} />;
  if (orderQuery.error || !orderQuery.data || !id) return <Alert message={getErrorMessage(t, orderQuery.error, "purchasing.orders.errors.load")} showIcon type="error" />;
  const order = orderQuery.data;
  if (order.status !== 0) return <Alert message={t("purchasing.orders.submittedReadOnly")} showIcon type="warning" />;
  async function submit(values: PurchaseOrderInput) { await update.mutateAsync(values); navigate(purchasingRoutes.orderDetail(id!)); }
  return <section><Typography.Title level={2}>{t("purchasing.orders.editTitle")}</Typography.Title>{update.error ? <Alert className="page-alert" message={getErrorMessage(t, update.error, "purchasing.orders.errors.update")} showIcon type="error" /> : null}<Card><PurchaseOrderForm cancelLabel={t("purchasing.cancel")} errorMessageKey="purchasing.orders.errors.update" initialValues={{ supplierId: order.supplierId, lines: order.lines.map(line => ({ supplierProductId: line.supplierProductId, quantity: line.quantity })) }} isSubmitting={update.isPending} onCancel={() => navigate(purchasingRoutes.orderDetail(id!))} onSubmit={submit} submitLabel={t("purchasing.save")} /></Card></section>;
}
