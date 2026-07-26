import { Alert, Card, Spin } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import type { WarehouseInput } from "../api/warehouseTypes";
import { useUpdateWarehouse, useWarehouse } from "../api/useWarehouses";
import { WarehouseForm } from "../components/WarehouseForm";
import { warehouseRoutes } from "../warehouseConstants";

export function WarehouseEditPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const warehouseQuery = useWarehouse(id);
  const update = useUpdateWarehouse(id ?? "");
  const { goBack, returnTo } = useReturnDestination(warehouseRoutes.detail(id ?? ""));

  if (warehouseQuery.isLoading)
    return <Spin className="page-spinner" size="large" tip={t("warehouses.loadingOne")} />;
  if (warehouseQuery.error || !warehouseQuery.data || !id)
    return <Alert message={getErrorMessage(t, warehouseQuery.error, "warehouses.errors.load")} showIcon type="error" />;

  const warehouse = warehouseQuery.data;
  async function submit(values: WarehouseInput) {
    await update.mutateAsync(values);
    navigate(warehouseRoutes.detail(id!));
  }

  return (
    <FormPageLayout backLabel={warehouse.name} backTo={returnTo} title={t("warehouses.editTitle")}>
      {update.error ? (
        <Alert className="page-alert" message={getErrorMessage(t, update.error, "warehouses.errors.update")} showIcon type="error" />
      ) : null}
      <Card>
        <WarehouseForm
          cancelLabel={t("warehouses.cancel")}
          initialValues={{ code: warehouse.code, name: warehouse.name, description: warehouse.description ?? undefined }}
          isSubmitting={update.isPending}
          onCancel={goBack}
          onSubmit={submit}
          submitLabel={t("warehouses.save")}
        />
      </Card>
    </FormPageLayout>
  );
}
