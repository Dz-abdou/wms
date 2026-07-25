import { Form, Input } from "antd";
import { useTranslation } from "react-i18next";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import type { WarehouseInput } from "../api/warehouseTypes";
import { warehouseValidation } from "../warehouseConstants";

type Props = {
  cancelLabel?: string;
  initialValues?: WarehouseInput;
  isSubmitting: boolean;
  onCancel?: () => void;
  onSubmit: (values: WarehouseInput) => Promise<void>;
  submitLabel: string;
};

export function WarehouseForm({
  initialValues,
  isSubmitting,
  onCancel,
  onSubmit,
  submitLabel,
  cancelLabel,
}: Props) {
  const [form] = Form.useForm<WarehouseInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();

  async function handleSubmit(values: WarehouseInput) {
    try {
      await onSubmit(values);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) {
        feedback.notifyError(error, "warehouses.errors.create");
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
        label={t("warehouses.form.code")}
        name="code"
        rules={[
          {
            required: true,
            whitespace: true,
            message: t("warehouses.form.codeRequired"),
          },
          {
            max: warehouseValidation.maxCodeLength,
            message: t("warehouses.form.codeMax", {
              max: warehouseValidation.maxCodeLength,
            }),
          },
        ]}
      >
        <Input maxLength={warehouseValidation.maxCodeLength} />
      </Form.Item>
      <Form.Item
        label={t("warehouses.form.name")}
        name="name"
        rules={[
          {
            required: true,
            whitespace: true,
            message: t("warehouses.form.nameRequired"),
          },
          {
            max: warehouseValidation.maxNameLength,
            message: t("warehouses.form.nameMax", {
              max: warehouseValidation.maxNameLength,
            }),
          },
        ]}
      >
        <Input maxLength={warehouseValidation.maxNameLength} />
      </Form.Item>
      <Form.Item
        label={t("warehouses.form.description")}
        name="description"
        rules={[
          {
            max: warehouseValidation.maxDescriptionLength,
            message: t("warehouses.form.descriptionMax", {
              max: warehouseValidation.maxDescriptionLength,
            }),
          },
        ]}
      >
        <Input.TextArea
          maxLength={warehouseValidation.maxDescriptionLength}
          rows={4}
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
