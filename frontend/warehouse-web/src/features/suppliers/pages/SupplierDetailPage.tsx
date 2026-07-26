import { Alert, Button, Descriptions, Popconfirm, Spin } from "antd";
import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useSetSupplierStatus, useSupplier } from "../api/useSuppliers";
import { supplierRoutes } from "../supplierConstants";
import {
  DetailPageLayout,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function SupplierDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const supplierQuery = useSupplier(id);
  const setStatus = useSetSupplierStatus(id ?? "");
  const { returnTo } = useReturnDestination(supplierRoutes.list);

  if (supplierQuery.isLoading) {
    return <Spin className="page-spinner" size="large" tip={t("suppliers.loadingOne")} />;
  }

  if (supplierQuery.error || !supplierQuery.data || !id) {
    return <Alert message={getErrorMessage(t, supplierQuery.error, "suppliers.errors.load")} showIcon type="error" />;
  }

  const supplier = supplierQuery.data;
  const action = t(supplier.isActive ? "suppliers.deactivate" : "suppliers.activate");
  const missingValue = t("suppliers.missingValue");

  return (
    <DetailPageLayout
      actions={
        <>
          <RouteActionButton to={supplierRoutes.edit(id)}>
            {t("suppliers.edit")}
          </RouteActionButton>
          <Popconfirm
            cancelText={t("suppliers.cancel")}
            description={t("suppliers.confirmStatusDescription", {
              action: action.toLocaleLowerCase(i18n.language),
            })}
            okText={action}
            onConfirm={() => setStatus.mutateAsync(!supplier.isActive)}
            title={t("suppliers.confirmStatusTitle", { action })}
          >
            <Button danger={supplier.isActive} loading={setStatus.isPending}>
              {action}
            </Button>
          </Popconfirm>
        </>
      }
      backLabel={t("suppliers.title")}
      backTo={returnTo}
      title={supplier.name}
    >
      {setStatus.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, setStatus.error, "suppliers.errors.status")}
          showIcon
          type="error"
        />
      ) : null}
      <Descriptions bordered column={1}>
        <Descriptions.Item label={t("suppliers.table.code")}>{supplier.code}</Descriptions.Item>
        <Descriptions.Item label={t("suppliers.table.name")}>{supplier.name}</Descriptions.Item>
        <Descriptions.Item label={t("suppliers.table.email")}>{supplier.email ?? missingValue}</Descriptions.Item>
        <Descriptions.Item label={t("suppliers.table.phoneNumber")}>{supplier.phoneNumber ?? missingValue}</Descriptions.Item>
        <Descriptions.Item label={t("suppliers.table.address")}>{supplier.address ?? missingValue}</Descriptions.Item>
        <Descriptions.Item label={t("suppliers.table.status")}><ProductStatusTag isActive={supplier.isActive} /></Descriptions.Item>
        <Descriptions.Item label={t("suppliers.table.created")}>
          {formatDateTime(supplier.createdAtUtc, toAppLanguage(i18n.resolvedLanguage))}
        </Descriptions.Item>
        <Descriptions.Item label={t("suppliers.table.updated")}>
          {formatDateTime(supplier.updatedAtUtc, toAppLanguage(i18n.resolvedLanguage))}
        </Descriptions.Item>
      </Descriptions>
    </DetailPageLayout>
  );
}
