import { Button, Form, Input, InputNumber, Select, Space, Switch } from "antd";
import { useTranslation } from "react-i18next";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useProductCategories } from "../api/useProductCategories";
import type { ProductFormValues } from "../api/productTypes";
import { productValidation } from "../productConstants";

type ProductFormProps = {
  initialValues?: ProductFormValues;
  isSubmitting: boolean;
  onSubmit: (values: ProductFormValues) => Promise<void>;
  submitLabel: string;
};

const baseUnitOptions = ["EA", "KG", "G", "L", "ML", "M", "CM", "MM"].map(
  (value) => ({ value, label: value }),
);

export function ProductForm({
  initialValues,
  isSubmitting,
  onSubmit,
  submitLabel,
}: ProductFormProps) {
  const [form] = Form.useForm<ProductFormValues>();
  const { t } = useTranslation();
  const categories = useProductCategories();
  const feedback = useApiFeedback();

  async function handleSubmit(values: ProductFormValues) {
    try {
      await onSubmit({
        ...values,
        categoryId: values.categoryId || undefined,
        unitConversions: values.unitConversions ?? [],
        measurements: hasMeasurements(values) ? values.measurements : undefined,
      });
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) {
        feedback.notifyError(error, "products.errors.create");
      }
    }
  }

  return (
    <Form<ProductFormValues>
      form={form}
      initialValues={{
        baseUnitOfMeasure: "EA",
        unitConversions: [],
        ...initialValues,
      }}
      layout="vertical"
      onFinish={handleSubmit}
      requiredMark="optional"
    >
      <Form.Item
        label={t("products.form.sku")}
        name="sku"
        rules={[
          {
            required: true,
            whitespace: true,
            message: t("products.form.skuRequired"),
          },
          {
            max: productValidation.maxSkuLength,
            message: t("products.form.skuMax", {
              max: productValidation.maxSkuLength,
            }),
          },
        ]}
      >
        <Input
          autoComplete="off"
          maxLength={productValidation.maxSkuLength}
          placeholder={t("products.form.skuPlaceholder")}
        />
      </Form.Item>
      <Form.Item
        label={t("products.form.name")}
        name="name"
        rules={[
          {
            required: true,
            whitespace: true,
            message: t("products.form.nameRequired"),
          },
          {
            max: productValidation.maxNameLength,
            message: t("products.form.nameMax", {
              max: productValidation.maxNameLength,
            }),
          },
        ]}
      >
        <Input
          maxLength={productValidation.maxNameLength}
          placeholder={t("products.form.namePlaceholder")}
        />
      </Form.Item>
      <Form.Item
        label={t("products.form.description")}
        name="description"
        rules={[
          {
            max: productValidation.maxDescriptionLength,
            message: t("products.form.descriptionMax", {
              max: productValidation.maxDescriptionLength,
            }),
          },
        ]}
      >
        <Input.TextArea
          maxLength={productValidation.maxDescriptionLength}
          rows={4}
          showCount
        />
      </Form.Item>
      <Form.Item
        label={t("products.form.baseUnit")}
        name="baseUnitOfMeasure"
        rules={[
          { required: true, message: t("products.form.baseUnitRequired") },
        ]}
      >
        <Select options={baseUnitOptions} />
      </Form.Item>
      <Form.Item label={t("products.form.category")} name="categoryId">
        <Select
          allowClear
          loading={categories.isLoading}
          options={categories.data?.items.map((category) => ({
            value: category.id,
            label: `${category.code} — ${category.name}`,
          }))}
        />
      </Form.Item>
      <Form.List name="unitConversions">
        {(fields, { add, remove }) => (
          <>
            <label>{t("products.form.conversions")}</label>
            {fields.map((field) => (
              <Space key={field.key} align="baseline">
                <Form.Item
                  {...field}
                  name={[field.name, "unitOfMeasure"]}
                  rules={[
                    {
                      required: true,
                      message: t("products.form.conversionUnitRequired"),
                    },
                  ]}
                >
                  <Input placeholder={t("products.form.conversionUnit")} />
                </Form.Item>
                <Form.Item
                  {...field}
                  name={[field.name, "quantityInBaseUnit"]}
                  rules={[
                    {
                      required: true,
                      type: "number",
                      min: 0.000001,
                      message: t("products.form.conversionQuantityRequired"),
                    },
                  ]}
                >
                  <InputNumber
                    min={0.000001}
                    precision={6}
                    placeholder={t("products.form.conversionQuantity")}
                  />
                </Form.Item>
                <Form.Item
                  {...field}
                  name={[field.name, "allowsFractionalQuantity"]}
                  valuePropName="checked"
                >
                  <Space>
                    <Switch
                      aria-label={t(
                        "products.form.conversionAllowsFractionalQuantity",
                      )}
                    />
                    {t("products.form.conversionAllowsFractionalQuantity")}
                  </Space>
                </Form.Item>
                <Button onClick={() => remove(field.name)}>
                  {t("products.form.removeConversion")}
                </Button>
              </Space>
            ))}
            <Button onClick={() => add({ allowsFractionalQuantity: false })}>
              {t("products.form.addConversion")}
            </Button>
          </>
        )}
      </Form.List>
      <Form.Item
        label={t("products.form.netWeight")}
        name={["measurements", "netWeight"]}
      >
        <InputNumber min={0.001} precision={3} />
      </Form.Item>
      <Form.Item
        label={t("products.form.grossWeight")}
        name={["measurements", "grossWeight"]}
      >
        <InputNumber min={0.001} precision={3} />
      </Form.Item>
      <Form.Item
        label={t("products.form.weightUnit")}
        name={["measurements", "weightUnitOfMeasure"]}
      >
        <Select
          allowClear
          options={["KG", "G", "LB"].map((value) => ({ value, label: value }))}
        />
      </Form.Item>
      <Form.Item
        label={t("products.form.length")}
        name={["measurements", "length"]}
      >
        <InputNumber min={0.001} precision={3} />
      </Form.Item>
      <Form.Item
        label={t("products.form.width")}
        name={["measurements", "width"]}
      >
        <InputNumber min={0.001} precision={3} />
      </Form.Item>
      <Form.Item
        label={t("products.form.height")}
        name={["measurements", "height"]}
      >
        <InputNumber min={0.001} precision={3} />
      </Form.Item>
      <Form.Item
        label={t("products.form.dimensionUnit")}
        name={["measurements", "dimensionUnitOfMeasure"]}
      >
        <Select
          allowClear
          options={["M", "CM", "MM"].map((value) => ({ value, label: value }))}
        />
      </Form.Item>
      <Button htmlType="submit" loading={isSubmitting} type="primary">
        {submitLabel}
      </Button>
    </Form>
  );
}

function hasMeasurements(values: ProductFormValues) {
  const measurements = values.measurements;
  return Boolean(
    measurements &&
    Object.values(measurements).some(
      (value) => value !== undefined && value !== null && value !== "",
    ),
  );
}
