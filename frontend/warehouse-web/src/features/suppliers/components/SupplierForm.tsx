import { useEffect } from "react";
import { Form, Input, Select } from "antd";
import { useTranslation } from "react-i18next";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { usePurchasingCurrencies } from "../../purchasing/api/usePurchasing";
import type { SupplierInput } from "../api/supplierTypes";
import { supplierValidation } from "../supplierConstants";

type Props = {
  cancelLabel?: string;
  initialValues?: SupplierInput;
  isSubmitting: boolean;
  onCancel?: () => void;
  onSubmit: (values: SupplierInput) => Promise<void>;
  submitLabel: string;
  errorMessageKey: string;
};

export function SupplierForm({
  initialValues,
  isSubmitting,
  onCancel,
  onSubmit,
  submitLabel,
  errorMessageKey,
  cancelLabel,
}: Props) {
  const [form] = Form.useForm<SupplierInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const currencies = usePurchasingCurrencies();

  useEffect(() => {
    if (
      initialValues?.defaultCurrencyCode ||
      form.getFieldValue("defaultCurrencyCode")
    ) {
      return;
    }

    const defaultCurrencyCode = currencies.data?.find(
      (currency) => currency.isDefault,
    )?.code;
    if (defaultCurrencyCode) {
      form.setFieldValue("defaultCurrencyCode", defaultCurrencyCode);
    }
  }, [currencies.data, form, initialValues?.defaultCurrencyCode]);

  async function handleSubmit(values: SupplierInput) {
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
        label={t("suppliers.form.code")}
        name="code"
        rules={[
          {
            required: true,
            whitespace: true,
            message: t("suppliers.form.codeRequired"),
          },
          {
            max: supplierValidation.maxCodeLength,
            message: t("suppliers.form.codeMax", {
              max: supplierValidation.maxCodeLength,
            }),
          },
        ]}
      >
        <Input maxLength={supplierValidation.maxCodeLength} />
      </Form.Item>
      <Form.Item
        label={t("suppliers.form.name")}
        name="name"
        rules={[
          {
            required: true,
            whitespace: true,
            message: t("suppliers.form.nameRequired"),
          },
          {
            max: supplierValidation.maxNameLength,
            message: t("suppliers.form.nameMax", {
              max: supplierValidation.maxNameLength,
            }),
          },
        ]}
      >
        <Input maxLength={supplierValidation.maxNameLength} />
      </Form.Item>
      <Form.Item
        label={t("suppliers.form.email")}
        name="email"
        rules={[
          {
            max: supplierValidation.maxEmailLength,
            message: t("suppliers.form.emailMax", {
              max: supplierValidation.maxEmailLength,
            }),
          },
        ]}
      >
        <Input maxLength={supplierValidation.maxEmailLength} type="email" />
      </Form.Item>
      <Form.Item
        label={t("suppliers.form.phoneNumber")}
        name="phoneNumber"
        rules={[
          {
            max: supplierValidation.maxPhoneNumberLength,
            message: t("suppliers.form.phoneNumberMax", {
              max: supplierValidation.maxPhoneNumberLength,
            }),
          },
        ]}
      >
        <Input maxLength={supplierValidation.maxPhoneNumberLength} />
      </Form.Item>
      <Form.Item
        label={t("suppliers.form.address")}
        name="address"
        rules={[
          {
            max: supplierValidation.maxAddressLength,
            message: t("suppliers.form.addressMax", {
              max: supplierValidation.maxAddressLength,
            }),
          },
        ]}
      >
        <Input.TextArea
          maxLength={supplierValidation.maxAddressLength}
          rows={4}
          showCount
        />
      </Form.Item>
      <Form.Item
        label={t("suppliers.form.defaultCurrency")}
        name="defaultCurrencyCode"
        rules={[
          {
            required: true,
            message: t("suppliers.form.defaultCurrencyRequired"),
          },
        ]}
      >
        <Select
          disabled={currencies.isLoading}
          options={currencies.data?.map((currency) => ({
            value: currency.code,
            label: currency.code,
          }))}
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
