import { Alert, Card, Form, Input, InputNumber, Spin } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import {
  useCreateCurrency,
  useCurrency,
  useUpdateCurrency,
} from "../api/usePurchasing";
import type { CurrencyInput } from "../api/purchasingTypes";
import { purchasingRoutes } from "../purchasingConstants";
export function CurrencyFormPage({ editing }: { editing: boolean }) {
  const { id } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const feedback = useApiFeedback();
  const currencyQuery = useCurrency(editing ? id : undefined);
  const create = useCreateCurrency();
  const update = useUpdateCurrency();
  const [form] = Form.useForm<CurrencyInput>();
  const { goBack, returnTo } = useReturnDestination(
    purchasingRoutes.currencies,
  );
  const currency = currencyQuery.data;
  if (currencyQuery.isLoading)
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("masterData.currencies.loading")}
      />
    );
  if (editing && !currency)
    return <Alert message={t("errors.requestFailed")} showIcon type="error" />;
  async function submit(values: CurrencyInput) {
    try {
      if (currency)
        await update.mutateAsync({
          id: currency.id,
          input: {
            name: values.name,
            symbol: values.symbol,
            decimalPlaces: values.decimalPlaces,
          },
        });
      else await create.mutateAsync(values);
      navigate(returnTo);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, "masterData.currencies.errors.save");
    }
  }
  return (
    <FormPageLayout
      backLabel={t("masterData.currencies.title")}
      backTo={returnTo}
      title={t(currency ? "masterData.edit" : "masterData.currencies.new")}
    >
      <Card>
        <Form
          form={form}
          initialValues={currency ?? { decimalPlaces: 2 }}
          layout="vertical"
          onFinish={submit}
        >
          <Form.Item
            label={t("masterData.code")}
            name="code"
            rules={[{ required: true }]}
          >
            <Input disabled={Boolean(currency)} maxLength={3} />
          </Form.Item>
          <Form.Item
            label={t("masterData.name")}
            name="name"
            rules={[{ required: true }]}
          >
            <Input />
          </Form.Item>
          <Form.Item label={t("masterData.symbol")} name="symbol">
            <Input />
          </Form.Item>
          <Form.Item
            label={t("masterData.decimals")}
            name="decimalPlaces"
            rules={[{ required: true }]}
          >
            <InputNumber min={0} max={4} />
          </Form.Item>
          <FormPageActions
            cancelLabel={t("ui.cancel")}
            isSubmitting={create.isPending || update.isPending}
            onCancel={goBack}
            submitLabel={t("masterData.save")}
          />
        </Form>
      </Card>
    </FormPageLayout>
  );
}
