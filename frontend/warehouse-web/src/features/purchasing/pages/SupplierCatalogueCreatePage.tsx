import { Alert, Card } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useCreateSupplierProduct } from "../api/usePurchasing";
import type { SupplierProductInput, UpdateSupplierProductInput } from "../api/purchasingTypes";
import { SupplierProductForm } from "../components/SupplierProductForm";
import { purchasingRoutes } from "../purchasingConstants";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function SupplierCatalogueCreatePage() {
  const { t } = useTranslation(); const navigate = useNavigate(); const create = useCreateSupplierProduct();
  const { goBack, returnTo } = useReturnDestination(purchasingRoutes.catalogue);
  async function submit(values: SupplierProductInput | UpdateSupplierProductInput) { await create.mutateAsync(values as SupplierProductInput); navigate(purchasingRoutes.catalogue); }
  return <FormPageLayout backLabel={t("purchasing.catalogue.title")} backTo={returnTo} title={t("purchasing.catalogue.createTitle")}>{create.error ? <Alert className="page-alert" message={getErrorMessage(t, create.error, "purchasing.catalogue.errors.create")} showIcon type="error" /> : null}<Card><SupplierProductForm cancelLabel={t("purchasing.cancel")} errorMessageKey="purchasing.catalogue.errors.create" isSubmitting={create.isPending} onCancel={goBack} onSubmit={submit} submitLabel={t("purchasing.catalogue.create")} /></Card></FormPageLayout>;
}
