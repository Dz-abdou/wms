import { Button, Form, InputNumber, Select, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import { useSuppliers } from "../../suppliers/api/useSuppliers";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useSupplierProducts } from "../api/usePurchasing";
import type {
  PurchaseOrderInput,
  SupplierProduct,
} from "../api/purchasingTypes";

type Props = {
  initialValues?: PurchaseOrderInput;
  isSubmitting: boolean;
  onSubmit: (values: PurchaseOrderInput) => Promise<void>;
  submitLabel: string;
  errorMessageKey: string;
};
type PurchaseOrderLineRow = {
  key: number;
  name: number;
  supplierProduct?: SupplierProduct;
};

export function PurchaseOrderForm({
  initialValues,
  isSubmitting,
  onSubmit,
  submitLabel,
  errorMessageKey,
}: Props) {
  const [form] = Form.useForm<PurchaseOrderInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const supplierId = Form.useWatch("supplierId", form);
  const suppliers = useSuppliers(1, 100);
  const catalogue = useSupplierProducts(1, 100, supplierId);
  const lines = Form.useWatch("lines", form);

  async function submit(values: PurchaseOrderInput) {
    try {
      await onSubmit({ ...values, lines: values.lines ?? [] });
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, errorMessageKey);
    }
  }

  const catalogueItems =
    catalogue.data?.items.filter((item) => item.isActive) ?? [];
  const catalogueOptions = catalogueItems.map((item) => ({
    value: item.id,
    label: `${item.productSku} — ${item.productName}`,
  }));

  return (
    <Form
      form={form}
      initialValues={initialValues}
      layout="vertical"
      onFinish={submit}
      requiredMark="optional"
      onValuesChange={(changedValues) => {
        if (changedValues.supplierId) form.setFieldsValue({ lines: [] });
      }}
    >
      <Form.Item
        label={t("purchasing.orders.supplier")}
        name="supplierId"
        rules={[
          { required: true, message: t("purchasing.orders.supplierRequired") },
        ]}
      >
        <Select
          options={suppliers.data?.items
            .filter((supplier) => supplier.isActive)
            .map((supplier) => ({
              value: supplier.id,
              label: `${supplier.code} — ${supplier.name}`,
            }))}
        />
      </Form.Item>
      <Form.List name="lines">
        {(fields, { add, remove }) => {
          const rows: PurchaseOrderLineRow[] = fields.map((field) => ({
            key: field.key,
            name: field.name,
            supplierProduct: catalogueItems.find(
              (item) => item.id === lines?.[field.name]?.supplierProductId,
            ),
          }));
          const columns: ColumnsType<PurchaseOrderLineRow> = [
            {
              title: t("purchasing.orders.catalogueItem"),
              key: "catalogueItem",
              width: 300,
              render: (_, row) => (
                <Form.Item
                  name={[row.name, "supplierProductId"]}
                  rules={[
                    {
                      required: true,
                      message: t("purchasing.orders.catalogueItemRequired"),
                    },
                  ]}
                  style={{ marginBottom: 0 }}
                >
                  <Select
                    aria-label={t("purchasing.orders.catalogueItem")}
                    disabled={!supplierId || catalogue.isLoading}
                    options={catalogueOptions}
                    showSearch
                    optionFilterProp="label"
                  />
                </Form.Item>
              ),
            },
            {
              title: t("purchasing.orders.product"),
              key: "product",
              render: (_, row) =>
                row.supplierProduct
                  ? `${row.supplierProduct.productSku} — ${row.supplierProduct.productName}`
                  : "—",
            },
            {
              title: t("purchasing.orders.supplierSku"),
              key: "supplierSku",
              render: (_, row) => row.supplierProduct?.supplierSku ?? "—",
            },
            {
              title: t("purchasing.orders.purchaseUnit"),
              key: "purchaseUnit",
              render: (_, row) =>
                row.supplierProduct?.purchaseUnitOfMeasure ?? "—",
            },
            {
              title: t("purchasing.orders.minimumOrderQuantity"),
              key: "minimumOrderQuantity",
              align: "right",
              render: (_, row) =>
                row.supplierProduct?.minimumOrderQuantity ?? "—",
            },
            {
              title: t("purchasing.orders.quantity"),
              key: "quantity",
              width: 150,
              render: (_, row) => (
                <Form.Item
                  name={[row.name, "quantity"]}
                  rules={[
                    {
                      required: true,
                      message: t("purchasing.orders.quantityRequired"),
                    },
                  ]}
                  style={{ marginBottom: 0 }}
                >
                  <InputNumber
                    aria-label={t("purchasing.orders.quantity")}
                    min={0.000001}
                    precision={6}
                  />
                </Form.Item>
              ),
            },
            {
              title: t("purchasing.orders.unitPrice"),
              key: "unitPrice",
              align: "right",
              render: (_, row) =>
                row.supplierProduct
                  ? `${row.supplierProduct.unitPrice} ${row.supplierProduct.currencyCode}`
                  : "—",
            },
            {
              title: t("purchasing.orders.lineTotal"),
              key: "lineTotal",
              align: "right",
              render: (_, row) => {
                const quantity = lines?.[row.name]?.quantity;
                return row.supplierProduct && typeof quantity === "number"
                  ? `${(quantity * row.supplierProduct.unitPrice).toFixed(2)} ${row.supplierProduct.currencyCode}`
                  : "—";
              },
            },
            {
              title: t("purchasing.orders.actions"),
              key: "actions",
              render: (_, row) => (
                <Button danger onClick={() => remove(row.name)} type="text">
                  {t("purchasing.orders.removeLine")}
                </Button>
              ),
            },
          ];

          return (
            <>
              <Table
                columns={columns}
                dataSource={rows}
                pagination={false}
                rowKey="key"
                scroll={{ x: 1200 }}
              />
              <Button
                disabled={!supplierId}
                onClick={() => add()}
                type="dashed"
              >
                {t("purchasing.orders.addLine")}
              </Button>
            </>
          );
        }}
      </Form.List>
      <div className="form-submit">
        <Button htmlType="submit" loading={isSubmitting} type="primary">
          {submitLabel}
        </Button>
      </div>
    </Form>
  );
}
