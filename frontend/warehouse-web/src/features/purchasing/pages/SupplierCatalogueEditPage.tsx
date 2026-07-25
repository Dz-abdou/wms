import { Alert, Button, Card, Popconfirm, Space, Spin, Typography } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useSetSupplierProductStatus, useSupplierProduct, useUpdateSupplierProduct } from "../api/usePurchasing";
import type { SupplierProductInput, UpdateSupplierProductInput } from "../api/purchasingTypes";
import { SupplierProductForm } from "../components/SupplierProductForm";
import { purchasingRoutes } from "../purchasingConstants";

export function SupplierCatalogueEditPage() {
  const { id } = useParams(); const { i18n, t } = useTranslation(); const navigate = useNavigate(); const itemQuery = useSupplierProduct(id); const update = useUpdateSupplierProduct(id ?? ""); const setStatus = useSetSupplierProductStatus(id ?? "");
  if (itemQuery.isLoading) return <Spin className="page-spinner" size="large" tip={t("purchasing.catalogue.loadingOne")} />;
  if (itemQuery.error || !itemQuery.data || !id) return <Alert message={getErrorMessage(t, itemQuery.error, "purchasing.catalogue.errors.load")} showIcon type="error" />;
  const item = itemQuery.data;
  async function submit(values: SupplierProductInput | UpdateSupplierProductInput) { await update.mutateAsync(values as UpdateSupplierProductInput); navigate(purchasingRoutes.catalogue); }
  const action = t(item.isActive ? "purchasing.deactivate" : "purchasing.activate");
  return <section><div className="page-heading"><Typography.Title level={2}>{t("purchasing.catalogue.editTitle")}</Typography.Title><Space><Popconfirm cancelText={t("purchasing.cancel")} description={t("purchasing.catalogue.confirmStatusDescription", { action: action.toLocaleLowerCase(i18n.language) })} okText={action} onConfirm={() => setStatus.mutateAsync(!item.isActive)} title={t("purchasing.catalogue.confirmStatusTitle", { action })}><Button danger={item.isActive} loading={setStatus.isPending}>{action}</Button></Popconfirm></Space></div>{update.error ? <Alert className="page-alert" message={getErrorMessage(t, update.error, "purchasing.catalogue.errors.update")} showIcon type="error" /> : null}{setStatus.error ? <Alert className="page-alert" message={getErrorMessage(t, setStatus.error, "purchasing.catalogue.errors.status")} showIcon type="error" /> : null}<Card><SupplierProductForm cancelLabel={t("purchasing.cancel")} errorMessageKey="purchasing.catalogue.errors.update" initialValues={{ supplierId: item.supplierId, productId: item.productId, supplierSku: item.supplierSku ?? undefined, purchaseUnitOfMeasure: item.purchaseUnitOfMeasure, minimumOrderQuantity: item.minimumOrderQuantity, unitPrice: item.unitPrice, currencyCode: item.currencyCode }} isEditing isSubmitting={update.isPending} onCancel={() => navigate(purchasingRoutes.catalogue)} onSubmit={submit} submitLabel={t("purchasing.save")} /></Card></section>;
}
