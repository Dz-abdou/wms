import { Alert, Card, Typography } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useCreateSupplier } from "../api/useSuppliers";
import type { SupplierInput } from "../api/supplierTypes";
import { SupplierForm } from "../components/SupplierForm";
import { supplierRoutes } from "../supplierConstants";

export function SupplierCreatePage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const create = useCreateSupplier();

  async function submit(values: SupplierInput) {
    const supplier = await create.mutateAsync(values);
    navigate(supplierRoutes.detail(supplier.id));
  }

  return (
    <section>
      <Typography.Title level={2}>{t("suppliers.createTitle")}</Typography.Title>
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
          onCancel={() => navigate(supplierRoutes.list)}
          onSubmit={submit}
          submitLabel={t("suppliers.create")}
        />
      </Card>
    </section>
  );
}
