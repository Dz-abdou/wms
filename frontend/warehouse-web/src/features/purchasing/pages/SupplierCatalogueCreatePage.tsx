import { Alert, Card, Typography } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useCreateSupplierProduct } from "../api/usePurchasing";
import type { SupplierProductInput, UpdateSupplierProductInput } from "../api/purchasingTypes";
import { SupplierProductForm } from "../components/SupplierProductForm";
import { purchasingRoutes } from "../purchasingConstants";

export function SupplierCatalogueCreatePage() {
  const { t } = useTranslation(); const navigate = useNavigate(); const create = useCreateSupplierProduct();
  async function submit(values: SupplierProductInput | UpdateSupplierProductInput) { await create.mutateAsync(values as SupplierProductInput); navigate(purchasingRoutes.catalogue); }
  return <section><Typography.Title level={2}>{t("purchasing.catalogue.createTitle")}</Typography.Title>{create.error ? <Alert className="page-alert" message={getErrorMessage(t, create.error, "purchasing.catalogue.errors.create")} showIcon type="error" /> : null}<Card><SupplierProductForm cancelLabel={t("purchasing.cancel")} errorMessageKey="purchasing.catalogue.errors.create" isSubmitting={create.isPending} onCancel={() => navigate(purchasingRoutes.catalogue)} onSubmit={submit} submitLabel={t("purchasing.catalogue.create")} /></Card></section>;
}
