import { Alert, Card } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import type { WarehouseInput } from "../api/warehouseTypes";
import { useCreateWarehouse } from "../api/useWarehouses";
import { WarehouseForm } from "../components/WarehouseForm";
import { warehouseRoutes } from "../warehouseConstants";

export function WarehouseCreatePage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const create = useCreateWarehouse();
  const { goBack, returnTo } = useReturnDestination(warehouseRoutes.list);

  async function submit(values: WarehouseInput) {
    const warehouse = await create.mutateAsync(values);
    navigate(warehouseRoutes.detail(warehouse.id));
  }

  return (
    <FormPageLayout
      backLabel={t("warehouses.title")}
      backTo={returnTo}
      title={t("warehouses.createTitle")}
    >
      {create.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, create.error, "warehouses.errors.create")}
          showIcon
          type="error"
        />
      ) : null}
      <Card>
        <WarehouseForm
          cancelLabel={t("warehouses.cancel")}
          isSubmitting={create.isPending}
          onCancel={goBack}
          onSubmit={submit}
          submitLabel={t("warehouses.create")}
        />
      </Card>
    </FormPageLayout>
  );
}
