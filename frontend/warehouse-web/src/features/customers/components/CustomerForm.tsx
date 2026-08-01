import { useEffect } from "react";
import { Form, Input, Select } from "antd";
import { useTranslation } from "react-i18next";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { usePurchasingCurrencies } from "../../purchasing/api/usePurchasing";
import type { CustomerInput } from "../api/customerTypes";
import { customerValidation } from "../customerConstants";

type Props = {
  cancelLabel: string;
  errorMessageKey: string;
  initialValues?: CustomerInput;
  isSubmitting: boolean;
  onCancel: () => void;
  onSubmit: (values: CustomerInput) => Promise<void>;
  submitLabel: string;
};

export function CustomerForm({
  cancelLabel,
  errorMessageKey,
  initialValues,
  isSubmitting,
  onCancel,
  onSubmit,
  submitLabel,
}: Props) {
  const [form] = Form.useForm<CustomerInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const currencies = usePurchasingCurrencies();

  useEffect(() => {
    form.setFieldsValue(initialValues ?? {});
  }, [form, initialValues]);

  async function handleSubmit(values: CustomerInput) {
    try {
      await onSubmit(values);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) {
        feedback.notifyError(error, errorMessageKey);
      }
    }
  }

  return (
    <Form
      form={form}
      initialValues={initialValues}
      layout="vertical"
      onFinish={handleSubmit}
      requiredMark="optional"
    >
      <Form.Item
        label={t("customers.form.code")}
        name="code"
        rules={[
          {
            required: true,
            whitespace: true,
            message: t("customers.form.codeRequired"),
          },
          {
            max: customerValidation.maxCodeLength,
            message: t("customers.form.codeMax", {
              max: customerValidation.maxCodeLength,
            }),
          },
        ]}
      >
        <Input maxLength={customerValidation.maxCodeLength} />
      </Form.Item>
      <Form.Item
        label={t("customers.form.legalName")}
        name="legalName"
        rules={[
          {
            required: true,
            whitespace: true,
            message: t("customers.form.legalNameRequired"),
          },
          {
            max: customerValidation.maxLegalNameLength,
            message: t("customers.form.legalNameMax", {
              max: customerValidation.maxLegalNameLength,
            }),
          },
        ]}
      >
        <Input maxLength={customerValidation.maxLegalNameLength} />
      </Form.Item>
      <Form.Item
        label={t("customers.form.tradingName")}
        name="tradingName"
        rules={[
          {
            max: customerValidation.maxTradingNameLength,
            message: t("customers.form.tradingNameMax", {
              max: customerValidation.maxTradingNameLength,
            }),
          },
        ]}
      >
        <Input maxLength={customerValidation.maxTradingNameLength} />
      </Form.Item>
      <Form.Item
        label={t("customers.form.defaultCurrency")}
        name="defaultCurrencyCode"
      >
        <Select
          allowClear
          disabled={currencies.isLoading}
          options={currencies.data?.map((currency) => ({
            value: currency.code,
            label: currency.code,
          }))}
        />
      </Form.Item>
      <Form.Item
        label={t("customers.form.deliveryInstructions")}
        name="deliveryInstructions"
        rules={[
          {
            max: customerValidation.maxDeliveryInstructionsLength,
            message: t("customers.form.deliveryInstructionsMax", {
              max: customerValidation.maxDeliveryInstructionsLength,
            }),
          },
        ]}
      >
        <Input.TextArea
          maxLength={customerValidation.maxDeliveryInstructionsLength}
          rows={3}
          showCount
        />
      </Form.Item>
      <Form.Item
        label={t("customers.form.serviceNotes")}
        name="serviceNotes"
        rules={[
          {
            max: customerValidation.maxServiceNotesLength,
            message: t("customers.form.serviceNotesMax", {
              max: customerValidation.maxServiceNotesLength,
            }),
          },
        ]}
      >
        <Input.TextArea
          maxLength={customerValidation.maxServiceNotesLength}
          rows={3}
          showCount
        />
      </Form.Item>
      <FormPageActions
        cancelLabel={cancelLabel}
        isSubmitting={isSubmitting}
        onCancel={onCancel}
        submitLabel={submitLabel}
      />
    </Form>
  );
}
