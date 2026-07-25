import { Alert, Card, Spin, Typography } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useSupplier, useUpdateSupplier } from "../api/useSuppliers";
import type { SupplierInput } from "../api/supplierTypes";
import { SupplierForm } from "../components/SupplierForm";
import { supplierRoutes } from "../supplierConstants";

export function SupplierEditPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const supplierQuery = useSupplier(id);
  const update = useUpdateSupplier(id ?? "");

  if (supplierQuery.isLoading) {
    return <Spin className="page-spinner" size="large" tip={t("suppliers.loadingOne")} />;
  }

  if (supplierQuery.error || !supplierQuery.data || !id) {
    return <Alert message={getErrorMessage(t, supplierQuery.error, "suppliers.errors.load")} showIcon type="error" />;
  }

  async function submit(values: SupplierInput) {
    await update.mutateAsync(values);
    navigate(supplierRoutes.detail(id!));
  }

  const supplier = supplierQuery.data;
  return (
    <section>
      <Typography.Title level={2}>{t("suppliers.editTitle")}</Typography.Title>
      {update.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, update.error, "suppliers.errors.update")}
          showIcon
          type="error"
        />
      ) : null}
      <Card>
        <SupplierForm
          cancelLabel={t("suppliers.cancel")}
          errorMessageKey="suppliers.errors.update"
          initialValues={{
            code: supplier.code,
            name: supplier.name,
            email: supplier.email ?? undefined,
            phoneNumber: supplier.phoneNumber ?? undefined,
            address: supplier.address ?? undefined,
          }}
          isSubmitting={update.isPending}
          onCancel={() => navigate(supplierRoutes.detail(id!))}
          onSubmit={submit}
          submitLabel={t("suppliers.save")}
        />
      </Card>
    </section>
  );
}
