import { Button, Form, InputNumber, Select, Space } from "antd";
import { useTranslation } from "react-i18next";
import { useSuppliers } from "../../suppliers/api/useSuppliers";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useSupplierProducts } from "../api/usePurchasing";
import type { PurchaseOrderInput } from "../api/purchasingTypes";

type Props = { initialValues?: PurchaseOrderInput; isSubmitting: boolean; onSubmit: (values: PurchaseOrderInput) => Promise<void>; submitLabel: string; errorMessageKey: string };

export function PurchaseOrderForm({ initialValues, isSubmitting, onSubmit, submitLabel, errorMessageKey }: Props) {
  const [form] = Form.useForm<PurchaseOrderInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const supplierId = Form.useWatch("supplierId", form);
  const suppliers = useSuppliers(1, 100);
  const catalogue = useSupplierProducts(1, 100, supplierId);

  async function submit(values: PurchaseOrderInput) {
    try {
      await onSubmit({ ...values, lines: values.lines ?? [] });
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) feedback.notifyError(error, errorMessageKey);
    }
  }

  const catalogueOptions = catalogue.data?.items.filter(item => item.isActive).map(item => ({ value: item.id, label: `${item.productSku} — ${item.productName} (${item.purchaseUnitOfMeasure}, ${item.unitPrice} ${item.currencyCode})` }));
  return <Form form={form} initialValues={initialValues} layout="vertical" onFinish={submit} requiredMark="optional">
    <Form.Item label={t("purchasing.orders.supplier")} name="supplierId" rules={[{ required: true, message: t("purchasing.orders.supplierRequired") }]}>
      <Select options={suppliers.data?.items.filter(supplier => supplier.isActive).map(supplier => ({ value: supplier.id, label: `${supplier.code} — ${supplier.name}` }))} />
    </Form.Item>
    <Form.List name="lines">
      {(fields, { add, remove }) => <>
        {fields.map(field => <Space key={field.key} align="baseline" className="purchase-order-line">
          <Form.Item label={t("purchasing.orders.catalogueItem")} name={[field.name, "supplierProductId"]} rules={[{ required: true, message: t("purchasing.orders.catalogueItemRequired") }]}><Select disabled={!supplierId || catalogue.isLoading} options={catalogueOptions} /></Form.Item>
          <Form.Item label={t("purchasing.orders.quantity")} name={[field.name, "quantity"]} rules={[{ required: true, message: t("purchasing.orders.quantityRequired") }]}><InputNumber min={0.000001} precision={6} /></Form.Item>
          <Button onClick={() => remove(field.name)}>{t("purchasing.orders.removeLine")}</Button>
        </Space>)}
        <Button onClick={() => add()} type="dashed">{t("purchasing.orders.addLine")}</Button>
      </>}
    </Form.List>
    <div className="form-submit"><Button htmlType="submit" loading={isSubmitting} type="primary">{submitLabel}</Button></div>
  </Form>;
}
