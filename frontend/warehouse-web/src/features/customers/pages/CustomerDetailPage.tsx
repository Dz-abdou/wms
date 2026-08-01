import {
  Alert,
  Button,
  Descriptions,
  Divider,
  Popconfirm,
  Spin,
  Typography,
} from "antd";
import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import {
  DetailPageLayout,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useCustomer, useSetCustomerStatus } from "../api/useCustomers";
import { CustomerAddressManager } from "../components/CustomerAddressManager";
import { CustomerContactManager } from "../components/CustomerContactManager";
import { customerRoutes } from "../customerConstants";

export function CustomerDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const customerQuery = useCustomer(id);
  const setStatus = useSetCustomerStatus(id ?? "");
  const { returnTo } = useReturnDestination(customerRoutes.list);

  if (customerQuery.isLoading)
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("customers.loadingOne")}
      />
    );
  if (customerQuery.error || !customerQuery.data || !id)
    return (
      <Alert
        message={getErrorMessage(
          t,
          customerQuery.error,
          "customers.errors.load",
        )}
        showIcon
        type="error"
      />
    );

  const customer = customerQuery.data;
  const action = t(
    customer.isActive ? "customers.deactivate" : "customers.activate",
  );
  const missingValue = t("customers.missingValue");

  return (
    <DetailPageLayout
      actions={
        <>
          <RouteActionButton to={customerRoutes.edit(id)}>
            {t("customers.edit")}
          </RouteActionButton>
          <Popconfirm
            cancelText={t("customers.cancel")}
            description={t("customers.confirmStatusDescription", {
              action: action.toLocaleLowerCase(i18n.language),
            })}
            okText={action}
            onConfirm={() => setStatus.mutateAsync(!customer.isActive)}
            title={t("customers.confirmStatusTitle", { action })}
          >
            <Button danger={customer.isActive} loading={setStatus.isPending}>
              {action}
            </Button>
          </Popconfirm>
        </>
      }
      backLabel={t("customers.title")}
      backTo={returnTo}
      title={customer.tradingName ?? customer.legalName}
    >
      {setStatus.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            setStatus.error,
            "customers.errors.status",
          )}
          showIcon
          type="error"
        />
      ) : null}
      <Descriptions bordered column={1}>
        <Descriptions.Item label={t("customers.table.code")}>
          {customer.code}
        </Descriptions.Item>
        <Descriptions.Item label={t("customers.table.legalName")}>
          {customer.legalName}
        </Descriptions.Item>
        <Descriptions.Item label={t("customers.table.tradingName")}>
          {customer.tradingName ?? missingValue}
        </Descriptions.Item>
        <Descriptions.Item label={t("customers.table.defaultCurrency")}>
          {customer.defaultCurrencyCode ?? missingValue}
        </Descriptions.Item>
        <Descriptions.Item label={t("customers.form.deliveryInstructions")}>
          {customer.deliveryInstructions ?? missingValue}
        </Descriptions.Item>
        <Descriptions.Item label={t("customers.form.serviceNotes")}>
          {customer.serviceNotes ?? missingValue}
        </Descriptions.Item>
        <Descriptions.Item label={t("customers.table.status")}>
          <ProductStatusTag isActive={customer.isActive} />
        </Descriptions.Item>
        <Descriptions.Item label={t("customers.table.created")}>
          {formatDateTime(
            customer.createdAtUtc,
            toAppLanguage(i18n.resolvedLanguage),
          )}
        </Descriptions.Item>
        <Descriptions.Item label={t("customers.table.updated")}>
          {formatDateTime(
            customer.updatedAtUtc,
            toAppLanguage(i18n.resolvedLanguage),
          )}
        </Descriptions.Item>
      </Descriptions>
      <Divider />
      <Typography.Title level={3}>
        {t("customers.contacts.title")}
      </Typography.Title>
      <CustomerContactManager
        contacts={customer.contacts}
        customerId={customer.id}
      />
      <Divider />
      <Typography.Title level={3}>
        {t("customers.addresses.title")}
      </Typography.Title>
      <CustomerAddressManager
        addresses={customer.addresses}
        customerId={customer.id}
      />
    </DetailPageLayout>
  );
}
