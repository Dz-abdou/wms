import { Alert, Card, Spin } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useCustomer, useUpdateCustomer } from "../api/useCustomers";
import type { CustomerInput } from "../api/customerTypes";
import { CustomerForm } from "../components/CustomerForm";
import { customerRoutes } from "../customerConstants";

export function CustomerEditPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const customerQuery = useCustomer(id);
  const update = useUpdateCustomer(id ?? "");
  const { goBack, returnTo } = useReturnDestination(
    customerRoutes.detail(id ?? ""),
  );

  if (customerQuery.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("customers.loadingOne")}
      />
    );
  }

  if (customerQuery.error || !customerQuery.data || !id) {
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
  }

  const customer = customerQuery.data;
  async function submit(values: CustomerInput) {
    await update.mutateAsync(values);
    navigate(customerRoutes.detail(id!));
  }

  return (
    <FormPageLayout
      backLabel={customer.legalName}
      backTo={returnTo}
      title={t("customers.editTitle")}
    >
      {update.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, update.error, "customers.errors.update")}
          showIcon
          type="error"
        />
      ) : null}
      <Card>
        <CustomerForm
          cancelLabel={t("customers.cancel")}
          errorMessageKey="customers.errors.update"
          initialValues={{
            code: customer.code,
            legalName: customer.legalName,
            tradingName: customer.tradingName ?? undefined,
            defaultCurrencyCode: customer.defaultCurrencyCode ?? undefined,
            deliveryInstructions: customer.deliveryInstructions ?? undefined,
            serviceNotes: customer.serviceNotes ?? undefined,
          }}
          isSubmitting={update.isPending}
          onCancel={goBack}
          onSubmit={submit}
          submitLabel={t("customers.save")}
        />
      </Card>
    </FormPageLayout>
  );
}
