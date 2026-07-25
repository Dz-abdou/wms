import { Button, Form, Input } from "antd";
import { useTranslation } from "react-i18next";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import type { SupplierInput } from "../api/supplierTypes";
import { supplierValidation } from "../supplierConstants";

type Props = {
  initialValues?: SupplierInput;
  isSubmitting: boolean;
  onSubmit: (values: SupplierInput) => Promise<void>;
  submitLabel: string;
  errorMessageKey: string;
};

export function SupplierForm({
  initialValues,
  isSubmitting,
  onSubmit,
  submitLabel,
  errorMessageKey,
}: Props) {
  const [form] = Form.useForm<SupplierInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();

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
      <Button htmlType="submit" loading={isSubmitting} type="primary">
        {submitLabel}
      </Button>
    </Form>
  );
}
