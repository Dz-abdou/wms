import { Alert, Card } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useCreateCustomer } from "../api/useCustomers";
import type { CustomerInput } from "../api/customerTypes";
import { CustomerForm } from "../components/CustomerForm";
import { customerRoutes } from "../customerConstants";

export function CustomerCreatePage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const create = useCreateCustomer();
  const { goBack, returnTo } = useReturnDestination(customerRoutes.list);

  async function submit(values: CustomerInput) {
    const customer = await create.mutateAsync(values);
    navigate(customerRoutes.detail(customer.id));
  }

  return (
    <FormPageLayout
      backLabel={t("customers.title")}
      backTo={returnTo}
      title={t("customers.createTitle")}
    >
      {create.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, create.error, "customers.errors.create")}
          showIcon
          type="error"
        />
      ) : null}
      <Card>
        <CustomerForm
          cancelLabel={t("customers.cancel")}
          errorMessageKey="customers.errors.create"
          isSubmitting={create.isPending}
          onCancel={goBack}
          onSubmit={submit}
          submitLabel={t("customers.create")}
        />
      </Card>
    </FormPageLayout>
  );
}
