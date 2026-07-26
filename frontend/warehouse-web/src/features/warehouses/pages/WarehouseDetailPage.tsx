import { Alert, Button, Descriptions, Popconfirm, Spin } from "antd";
import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  DetailPageLayout,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import { useSetWarehouseStatus, useWarehouse } from "../api/useWarehouses";
import { warehouseRoutes } from "../warehouseConstants";

export function WarehouseDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const warehouseQuery = useWarehouse(id);
  const setStatus = useSetWarehouseStatus(id ?? "");
  const { returnTo } = useReturnDestination(warehouseRoutes.list);
  if (warehouseQuery.isLoading)
    return <Spin className="page-spinner" size="large" tip={t("warehouses.loadingOne")} />;
  if (warehouseQuery.error || !warehouseQuery.data || !id)
    return <Alert message={getErrorMessage(t, warehouseQuery.error, "warehouses.errors.load")} showIcon type="error" />;

  const warehouse = warehouseQuery.data;
  const action = t(warehouse.isActive ? "warehouses.deactivate" : "warehouses.activate");
  return (
    <DetailPageLayout
      actions={
        <>
          <RouteActionButton to={warehouseRoutes.edit(id)}>{t("warehouses.edit")}</RouteActionButton>
          <Popconfirm
            cancelText={t("warehouses.cancel")}
            description={t("warehouses.confirmStatusDescription", { action: action.toLocaleLowerCase(i18n.language) })}
            okText={action}
            onConfirm={() => setStatus.mutateAsync(!warehouse.isActive)}
            title={t("warehouses.confirmStatusTitle", { action })}
          >
            <Button danger={warehouse.isActive} loading={setStatus.isPending}>{action}</Button>
          </Popconfirm>
        </>
      }
      backLabel={t("warehouses.title")}
      backTo={returnTo}
      title={warehouse.name}
    >
      {setStatus.error ? <Alert className="page-alert" message={getErrorMessage(t, setStatus.error, "warehouses.errors.status")} showIcon type="error" /> : null}
      <Descriptions bordered column={1}>
        <Descriptions.Item label={t("warehouses.table.code")}>{warehouse.code}</Descriptions.Item>
        <Descriptions.Item label={t("warehouses.table.name")}>{warehouse.name}</Descriptions.Item>
        <Descriptions.Item label={t("warehouses.table.description")}>{warehouse.description ?? t("warehouses.missingDescription")}</Descriptions.Item>
        <Descriptions.Item label={t("warehouses.table.status")}><ProductStatusTag isActive={warehouse.isActive} /></Descriptions.Item>
        <Descriptions.Item label={t("warehouses.table.created")}>{formatDateTime(warehouse.createdAtUtc, toAppLanguage(i18n.resolvedLanguage))}</Descriptions.Item>
        <Descriptions.Item label={t("warehouses.table.updated")}>{formatDateTime(warehouse.updatedAtUtc, toAppLanguage(i18n.resolvedLanguage))}</Descriptions.Item>
      </Descriptions>
    </DetailPageLayout>
  );
}
