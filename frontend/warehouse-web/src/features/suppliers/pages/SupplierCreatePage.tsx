import { Alert, Card } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useCreateSupplier } from "../api/useSuppliers";
import type { SupplierInput } from "../api/supplierTypes";
import { SupplierForm } from "../components/SupplierForm";
import { supplierRoutes } from "../supplierConstants";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function SupplierCreatePage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const create = useCreateSupplier();
  const { goBack, returnTo } = useReturnDestination(supplierRoutes.list);

  async function submit(values: SupplierInput) {
    const supplier = await create.mutateAsync(values);
    navigate(supplierRoutes.detail(supplier.id));
  }

  return (
    <FormPageLayout
      backLabel={t("suppliers.title")}
      backTo={returnTo}
      title={t("suppliers.createTitle")}
    >
      {create.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, create.error, "suppliers.errors.create")}
          showIcon
          type="error"
        />
      ) : null}
      <Card>
        <SupplierForm
          cancelLabel={t("suppliers.cancel")}
          errorMessageKey="suppliers.errors.create"
          isSubmitting={create.isPending}
          onCancel={goBack}
          onSubmit={submit}
          submitLabel={t("suppliers.create")}
        />
      </Card>
    </FormPageLayout>
  );
}
