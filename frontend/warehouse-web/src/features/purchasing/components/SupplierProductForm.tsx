import { Button, Form, Input, InputNumber, Select } from "antd";
import { useTranslation } from "react-i18next";
import { useProducts } from "../../products/api/useProducts";
import { useSuppliers } from "../../suppliers/api/useSuppliers";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { purchasingValidation } from "../purchasingConstants";
import type { SupplierProductInput, UpdateSupplierProductInput } from "../api/purchasingTypes";

type Props = {
  initialValues?: SupplierProductInput;
  isEditing?: boolean;
  isSubmitting: boolean;
  onSubmit: (values: SupplierProductInput | UpdateSupplierProductInput) => Promise<void>;
  submitLabel: string;
  errorMessageKey: string;
};

export function SupplierProductForm({ initialValues, isEditing = false, isSubmitting, onSubmit, submitLabel, errorMessageKey }: Props) {
  const [form] = Form.useForm<SupplierProductInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const suppliers = useSuppliers(1, 100);
  const products = useProducts({ page: 1, pageSize: 100 });

  async function submit(values: SupplierProductInput) {
    const request = isEditing
      ? { supplierSku: values.supplierSku, purchaseUnitOfMeasure: values.purchaseUnitOfMeasure, minimumOrderQuantity: values.minimumOrderQuantity, unitPrice: values.unitPrice, currencyCode: values.currencyCode }
      : values;
    try {
      await onSubmit(request);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) feedback.notifyError(error, errorMessageKey);
    }
  }

  return <Form form={form} initialValues={initialValues} layout="vertical" onFinish={submit} requiredMark="optional">
    <Form.Item label={t("purchasing.catalogue.supplier")} name="supplierId" rules={[{ required: true, message: t("purchasing.catalogue.supplierRequired") }]}>
      <Select disabled={isEditing || suppliers.isLoading} options={suppliers.data?.items.filter(supplier => supplier.isActive).map(supplier => ({ value: supplier.id, label: `${supplier.code} — ${supplier.name}` }))} />
    </Form.Item>
    <Form.Item label={t("purchasing.catalogue.product")} name="productId" rules={[{ required: true, message: t("purchasing.catalogue.productRequired") }]}>
      <Select disabled={isEditing || products.isLoading} options={products.data?.items.filter(product => product.isActive).map(product => ({ value: product.id, label: `${product.sku} — ${product.name}` }))} />
    </Form.Item>
    <Form.Item label={t("purchasing.catalogue.supplierSku")} name="supplierSku" rules={[{ max: purchasingValidation.maxSupplierSkuLength, message: t("purchasing.catalogue.supplierSkuMax", { max: purchasingValidation.maxSupplierSkuLength }) }]}><Input maxLength={purchasingValidation.maxSupplierSkuLength} /></Form.Item>
    <Form.Item label={t("purchasing.catalogue.purchaseUnit")} name="purchaseUnitOfMeasure" rules={[{ required: true, whitespace: true, message: t("purchasing.catalogue.purchaseUnitRequired") }, { max: purchasingValidation.maxUnitOfMeasureLength, message: t("purchasing.catalogue.purchaseUnitMax", { max: purchasingValidation.maxUnitOfMeasureLength }) }]}><Input maxLength={purchasingValidation.maxUnitOfMeasureLength} /></Form.Item>
    <Form.Item label={t("purchasing.catalogue.minimumOrderQuantity")} name="minimumOrderQuantity" rules={[{ required: true, message: t("purchasing.catalogue.minimumOrderQuantityRequired") }]}><InputNumber min={0.000001} precision={6} /></Form.Item>
    <Form.Item label={t("purchasing.catalogue.unitPrice")} name="unitPrice" rules={[{ required: true, message: t("purchasing.catalogue.unitPriceRequired") }]}><InputNumber min={0} precision={4} /></Form.Item>
    <Form.Item label={t("purchasing.catalogue.currencyCode")} name="currencyCode" rules={[{ required: true, whitespace: true, message: t("purchasing.catalogue.currencyCodeRequired") }, { len: purchasingValidation.currencyCodeLength, message: t("purchasing.catalogue.currencyCodeLength", { length: purchasingValidation.currencyCodeLength }) }]}><Input maxLength={purchasingValidation.currencyCodeLength} /></Form.Item>
    <Button htmlType="submit" loading={isSubmitting} type="primary">{submitLabel}</Button>
  </Form>;
}
