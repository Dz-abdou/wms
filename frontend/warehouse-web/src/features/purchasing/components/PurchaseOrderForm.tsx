import { useEffect, useMemo } from "react";
import { Button, Form, Input, InputNumber, Select } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import {
  EditableFormListTable,
  type EditableFormListTableRow,
} from "../../../shared/components/EditableFormListTable";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { useSuppliers } from "../../suppliers/api/useSuppliers";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useSupplierProducts } from "../api/usePurchasing";
import type {
  PurchaseOrderInput,
  SupplierProduct,
} from "../api/purchasingTypes";

type Props = {
  cancelLabel?: string;
  initialValues?: PurchaseOrderInput;
  isSubmitting: boolean;
  onCancel?: () => void;
  onSubmit: (values: PurchaseOrderInput) => Promise<void>;
  submitLabel: string;
  errorMessageKey: string;
};
type PurchaseOrderLineRow = {
  supplierProduct?: SupplierProduct;
};

export function PurchaseOrderForm({
  initialValues,
  isSubmitting,
  onCancel,
  onSubmit,
  submitLabel,
  errorMessageKey,
  cancelLabel,
}: Props) {
  const [form] = Form.useForm<PurchaseOrderInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const supplierId = Form.useWatch("supplierId", form);
  const suppliers = useSuppliers({ page: 1, pageSize: 100 });
  const warehouses = useWarehouses({ page: 1, pageSize: 100 });
  const catalogue = useSupplierProducts({ page: 1, pageSize: 100, supplierId });
  const lines = Form.useWatch("lines", form);

  async function submit(values: PurchaseOrderInput) {
    try {
      await onSubmit({ ...values, lines: values.lines ?? [] });
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, errorMessageKey);
    }
  }

  const catalogueItems = useMemo(
    () => catalogue.data?.items.filter((item) => item.isActive) ?? [],
    [catalogue.data?.items],
  );
  const selectedCatalogueItems = useMemo(
    () =>
      lines
        ?.map((line) =>
          catalogueItems.find((item) => item.id === line.supplierProductId),
        )
        .filter((item): item is SupplierProduct => item !== undefined) ?? [],
    [catalogueItems, lines],
  );
  const selectedCurrencyCode = selectedCatalogueItems[0]?.currencyCode;
  const hasSelectedCatalogueItem = Boolean(
    lines?.some((line) => line.supplierProductId),
  );
  const compatibleCatalogueItems = selectedCurrencyCode
    ? catalogueItems.filter(
        (item) => item.currencyCode === selectedCurrencyCode,
      )
    : catalogueItems;
  const catalogueOptions = compatibleCatalogueItems.map((item) => ({
    value: item.id,
    label: `${item.productSku} — ${item.productName}`,
  }));

  useEffect(() => {
    if (!selectedCurrencyCode) {
      if (!hasSelectedCatalogueItem && form.getFieldValue("currencyCode")) {
        form.setFieldValue("currencyCode", undefined);
      }
      return;
    }

    const incompatibleLines = lines?.map((line) => {
      const catalogueItem = catalogueItems.find(
        (item) => item.id === line.supplierProductId,
      );
      return catalogueItem &&
        catalogueItem.currencyCode !== selectedCurrencyCode
        ? { ...line, supplierProductId: undefined }
        : line;
    });
    const hasIncompatibleLine = incompatibleLines?.some(
      (line, index) => line !== lines?.[index],
    );
    if (
      form.getFieldValue("currencyCode") !== selectedCurrencyCode ||
      hasIncompatibleLine
    ) {
      form.setFieldsValue({
        currencyCode: selectedCurrencyCode,
        ...(hasIncompatibleLine ? { lines: incompatibleLines } : {}),
      });
    }
  }, [
    catalogueItems,
    form,
    hasSelectedCatalogueItem,
    lines,
    selectedCurrencyCode,
  ]);
  const columns = (
    remove: (fieldName: number) => void,
  ): ColumnsType<PurchaseOrderLineRow & EditableFormListTableRow> => [
    {
      title: t("purchasing.orders.catalogueItem"),
      key: "catalogueItem",
      width: 300,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "supplierProductId"]}
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
      render: (_, row) => row.supplierProduct?.purchaseUnitOfMeasure ?? "—",
    },
    {
      title: t("purchasing.orders.minimumOrderQuantity"),
      key: "minimumOrderQuantity",
      align: "right",
      render: (_, row) => row.supplierProduct?.minimumOrderQuantity ?? "—",
    },
    {
      title: t("purchasing.orders.quantity"),
      key: "quantity",
      width: 150,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "quantity"]}
          rules={[
            {
              required: true,
              message: t("purchasing.orders.quantityRequired"),
            },
            {
              validator: (_, value) =>
                row.supplierProduct &&
                typeof value === "number" &&
                value < row.supplierProduct.minimumOrderQuantity
                  ? Promise.reject(
                      new Error(
                        t("purchasing.orders.quantityMinimum", {
                          minimumOrderQuantity:
                            row.supplierProduct.minimumOrderQuantity,
                        }),
                      ),
                    )
                  : Promise.resolve(),
            },
          ]}
          style={{ marginBottom: 0 }}
        >
          <InputNumber
            aria-label={t("purchasing.orders.quantity")}
            min={row.supplierProduct?.minimumOrderQuantity ?? 0.000001}
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
        const quantity = lines?.[row.fieldName]?.quantity;
        return row.supplierProduct && typeof quantity === "number"
          ? `${(quantity * row.supplierProduct.unitPrice).toFixed(2)} ${row.supplierProduct.currencyCode}`
          : "—";
      },
    },
    {
      title: t("purchasing.orders.actions"),
      key: "actions",
      render: (_, row) => (
        <Button danger onClick={() => remove(row.fieldName)} type="text">
          {t("purchasing.orders.removeLine")}
        </Button>
      ),
    },
  ];

  return (
    <Form
      form={form}
      initialValues={initialValues}
      layout="vertical"
      onFinish={submit}
      requiredMark="optional"
      onValuesChange={(changedValues) => {
        if ("supplierId" in changedValues) {
          form.setFieldsValue({ currencyCode: undefined, lines: [] });
        }
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
      <Form.Item
        label={t("purchasing.orders.warehouse")}
        name="destinationWarehouseId"
        rules={[{ required: true, message: t("errors.validationFailed") }]}
      >
        <Select
          options={warehouses.data?.items
            .filter((warehouse) => warehouse.isActive)
            .map((warehouse) => ({
              value: warehouse.id,
              label: `${warehouse.code} — ${warehouse.name}`,
            }))}
        />
      </Form.Item>
      <Form.Item
        label={t("purchasing.orders.currency")}
        name="currencyCode"
        rules={[{ required: true, message: t("errors.validationFailed") }]}
        extra={t("purchasing.orders.currencyFromCatalogue")}
      >
        <Input readOnly />
      </Form.Item>
      <Form.Item
        label={t("purchasing.orders.orderDate")}
        name="orderDate"
        rules={[{ required: true, message: t("errors.validationFailed") }]}
      >
        <Input type="date" />
      </Form.Item>
      <Form.Item
        label={t("purchasing.orders.expectedDeliveryDate")}
        name="expectedDeliveryDate"
      >
        <Input type="date" />
      </Form.Item>
      <Form.Item
        label={t("purchasing.orders.supplierReference")}
        name="supplierReference"
      >
        <Input />
      </Form.Item>
      <Form.Item label={t("purchasing.orders.notes")} name="notes">
        <Input.TextArea rows={3} />
      </Form.Item>
      <EditableFormListTable<PurchaseOrderLineRow>
        addDisabled={!supplierId}
        addLabel={t("purchasing.orders.addLine")}
        columns={columns}
        createRow={(field) => ({
          supplierProduct: catalogueItems.find(
            (item) => item.id === lines?.[field.name]?.supplierProductId,
          ),
        })}
        name="lines"
        scroll={{ x: 1200 }}
      />
      <FormPageActions
        cancelLabel={cancelLabel}
        isSubmitting={isSubmitting}
        onCancel={onCancel}
        submitLabel={submitLabel}
      />
    </Form>
  );
}
