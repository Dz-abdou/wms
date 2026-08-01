import { useMemo } from "react";
import { Button, Form, Input, InputNumber, Select } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import {
  EditableFormListTable,
  type EditableFormListTableRow,
} from "../../../shared/components/EditableFormListTable";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useCustomer, useCustomers } from "../../customers/api/useCustomers";
import { useProducts } from "../../products/api/useProducts";
import { usePurchasingCurrencies } from "../../purchasing/api/usePurchasing";
import type { Product } from "../../products/api/productTypes";
import type { SalesOrderInput } from "../api/salesTypes";

type Props = {
  initialValues?: SalesOrderInput;
  isSubmitting: boolean;
  onCancel?: () => void;
  onSubmit: (values: SalesOrderInput) => Promise<void>;
  submitLabel: string;
  errorMessageKey: string;
  cancelLabel?: string;
};
type LineRow = { product?: Product };

export function SalesOrderForm({
  initialValues,
  isSubmitting,
  onCancel,
  onSubmit,
  submitLabel,
  errorMessageKey,
  cancelLabel,
}: Props) {
  const [form] = Form.useForm<SalesOrderInput>();
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const customerId = Form.useWatch("customerId", form);
  const lines = Form.useWatch("lines", form);
  const customers = useCustomers({ page: 1, pageSize: 100 });
  const customer = useCustomer(customerId);
  const products = useProducts({ page: 1, pageSize: 100, isActive: true });
  const currencies = usePurchasingCurrencies();

  async function submit(values: SalesOrderInput) {
    try {
      await onSubmit({
        ...values,
        lines: values.lines ?? [],
        version: initialValues?.version,
      });
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, errorMessageKey);
    }
  }
  const productOptions = useMemo(
    () =>
      (products.data?.items ?? [])
        .filter((product) => product.isActive)
        .map((product) => ({
          value: product.id,
          label: `${product.sku} — ${product.name}`,
        })),
    [products.data?.items],
  );
  const columns = (
    remove: (fieldName: number) => void,
  ): ColumnsType<LineRow & EditableFormListTableRow> => [
    {
      title: t("sales.orders.product"),
      key: "product",
      width: 320,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "productId"]}
          rules={[
            { required: true, message: t("sales.orders.productRequired") },
          ]}
          style={{ marginBottom: 0 }}
        >
          <Select
            aria-label={t("sales.orders.product")}
            loading={products.isLoading}
            options={productOptions}
            showSearch
            optionFilterProp="label"
          />
        </Form.Item>
      ),
    },
    {
      title: t("sales.orders.unit"),
      key: "unit",
      width: 150,
      render: (_, row) => {
        const product = row.product;
        const options = product
          ? [
              {
                value: product.baseUnitOfMeasure,
                label: product.baseUnitOfMeasure,
              },
              ...product.unitConversions.map((conversion) => ({
                value: conversion.unitOfMeasure,
                label: conversion.unitOfMeasure,
              })),
            ]
          : [];
        return (
          <Form.Item
            name={[row.fieldName, "unitOfMeasure"]}
            rules={[
              { required: true, message: t("sales.orders.unitRequired") },
            ]}
            style={{ marginBottom: 0 }}
          >
            <Select
              aria-label={t("sales.orders.unit")}
              disabled={!product}
              options={options}
            />
          </Form.Item>
        );
      },
    },
    {
      title: t("sales.orders.quantity"),
      key: "quantity",
      width: 160,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "quantity"]}
          rules={[
            { required: true, message: t("sales.orders.quantityRequired") },
          ]}
          style={{ marginBottom: 0 }}
        >
          <InputNumber
            aria-label={t("sales.orders.quantity")}
            min={0.000001}
            precision={6}
          />
        </Form.Item>
      ),
    },
    {
      title: t("sales.orders.actions"),
      key: "actions",
      fixed: "right",
      width: 120,
      render: (_, row) => (
        <Button danger onClick={() => remove(row.fieldName)} type="text">
          {t("sales.orders.removeLine")}
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
      onValuesChange={(changed) => {
        if (changed.customerId) {
          const selected = customers.data?.items.find(
            (item) => item.id === changed.customerId,
          );
          form.setFieldsValue({
            shippingAddressId: undefined,
            currencyCode: selected?.defaultCurrencyCode ?? undefined,
          });
        }
      }}
    >
      <Form.Item
        label={t("sales.orders.customer")}
        name="customerId"
        rules={[
          { required: true, message: t("sales.orders.customerRequired") },
        ]}
      >
        <Select
          options={customers.data?.items
            .filter((item) => item.isActive)
            .map((item) => ({
              value: item.id,
              label: `${item.code} — ${item.tradingName ?? item.legalName}`,
            }))}
          showSearch
          optionFilterProp="label"
        />
      </Form.Item>
      <Form.Item
        label={t("sales.orders.shippingAddress")}
        name="shippingAddressId"
        rules={[
          {
            required: true,
            message: t("sales.orders.shippingAddressRequired"),
          },
        ]}
      >
        <Select
          disabled={!customerId || customer.isLoading}
          options={customer.data?.addresses
            .filter((address) => address.isShippingAddress)
            .map((address) => ({
              value: address.id,
              label: `${address.label} — ${address.addressLine1}, ${address.city}`,
            }))}
        />
      </Form.Item>
      <Form.Item
        label={t("sales.orders.currency")}
        name="currencyCode"
        rules={[{ required: true, message: t("errors.validationFailed") }]}
      >
        <Select
          options={currencies.data
            ?.filter((currency) => currency.isActive)
            .map((currency) => ({
              value: currency.code,
              label: `${currency.code} — ${currency.name}`,
            }))}
        />
      </Form.Item>
      <Form.Item
        label={t("sales.orders.orderDate")}
        name="orderDate"
        rules={[{ required: true, message: t("errors.validationFailed") }]}
      >
        <Input type="date" />
      </Form.Item>
      <Form.Item
        label={t("sales.orders.requestedShipDate")}
        name="requestedShipDate"
      >
        <Input type="date" />
      </Form.Item>
      <Form.Item
        label={t("sales.orders.customerReference")}
        name="customerReference"
      >
        <Input />
      </Form.Item>
      <Form.Item
        label={t("sales.orders.deliveryInstructions")}
        name="deliveryInstructions"
      >
        <Input.TextArea rows={3} />
      </Form.Item>
      <EditableFormListTable<LineRow>
        addLabel={t("sales.orders.addLine")}
        columns={columns}
        createRow={(field) => {
          const product = products.data?.items.find(
            (item) => item.id === lines?.[field.name]?.productId,
          );
          return { product };
        }}
        name="lines"
        scroll={{ x: 900 }}
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
