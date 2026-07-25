import { Button, Form, Input, InputNumber, Select } from "antd";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useProducts } from "../../products/api/useProducts";
import { useSuppliers } from "../../suppliers/api/useSuppliers";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { purchasingValidation } from "../purchasingConstants";
import { usePurchasingCurrencies } from "../api/usePurchasing";
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
  const currencies = usePurchasingCurrencies();
  const selectedProductId = Form.useWatch("productId", form);
  const selectedProduct = products.data?.items.find((product) => product.id === selectedProductId);

  useEffect(() => {
    const defaultCurrency = currencies.data?.find((currency) => currency.isDefault);
    if (!isEditing && defaultCurrency && !form.getFieldValue("currencyCode")) {
      form.setFieldsValue({ currencyCode: defaultCurrency.code });
    }
  }, [currencies.data, form, isEditing]);

  const purchaseUnitOptions = selectedProduct
    ? [
        { value: selectedProduct.baseUnitOfMeasure, label: t("purchasing.catalogue.purchaseUnitBase", { unit: selectedProduct.baseUnitOfMeasure }) },
        ...selectedProduct.unitConversions.map((conversion) => ({
          value: conversion.unitOfMeasure,
          label: t("purchasing.catalogue.purchaseUnitConversion", {
            unit: conversion.unitOfMeasure,
            quantity: conversion.quantityInBaseUnit,
            baseUnit: selectedProduct.baseUnitOfMeasure,
          }),
        })),
      ]
    : [];

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

  return <Form
    form={form}
    initialValues={initialValues}
    layout="vertical"
    onFinish={submit}
    onValuesChange={(changedValues) => {
      if ("productId" in changedValues) form.setFieldValue("purchaseUnitOfMeasure", undefined);
    }}
    requiredMark="optional"
  >
    <Form.Item label={t("purchasing.catalogue.supplier")} name="supplierId" rules={[{ required: true, message: t("purchasing.catalogue.supplierRequired") }]}>
      <Select disabled={isEditing || suppliers.isLoading} options={suppliers.data?.items.filter(supplier => supplier.isActive).map(supplier => ({ value: supplier.id, label: `${supplier.code} — ${supplier.name}` }))} />
    </Form.Item>
    <Form.Item label={t("purchasing.catalogue.product")} name="productId" rules={[{ required: true, message: t("purchasing.catalogue.productRequired") }]}>
      <Select disabled={isEditing || products.isLoading} options={products.data?.items.filter(product => product.isActive).map(product => ({ value: product.id, label: `${product.sku} — ${product.name}` }))} />
    </Form.Item>
    <Form.Item label={t("purchasing.catalogue.supplierSku")} name="supplierSku" rules={[{ max: purchasingValidation.maxSupplierSkuLength, message: t("purchasing.catalogue.supplierSkuMax", { max: purchasingValidation.maxSupplierSkuLength }) }]}><Input maxLength={purchasingValidation.maxSupplierSkuLength} /></Form.Item>
    <Form.Item label={t("purchasing.catalogue.purchaseUnit")} name="purchaseUnitOfMeasure" rules={[{ required: true, message: t("purchasing.catalogue.purchaseUnitRequired") }]}>
      <Select disabled={!selectedProduct || isSubmitting} loading={products.isLoading} options={purchaseUnitOptions} />
    </Form.Item>
    <Form.Item label={t("purchasing.catalogue.minimumOrderQuantity")} extra={t("purchasing.catalogue.minimumOrderQuantityHelp")} name="minimumOrderQuantity" rules={[{ required: true, message: t("purchasing.catalogue.minimumOrderQuantityRequired") }]}><InputNumber min={0.000001} precision={6} /></Form.Item>
    <Form.Item label={t("purchasing.catalogue.unitPrice")} extra={t("purchasing.catalogue.unitPriceHelp")} name="unitPrice" rules={[{ required: true, message: t("purchasing.catalogue.unitPriceRequired") }]}><InputNumber min={0} precision={4} /></Form.Item>
    <Form.Item label={t("purchasing.catalogue.currencyCode")} name="currencyCode" rules={[{ required: true, message: t("purchasing.catalogue.currencyCodeRequired") }]}>
      <Select disabled={currencies.isLoading || isSubmitting} loading={currencies.isLoading} options={currencies.data?.map((currency) => ({ value: currency.code, label: currency.code }))} />
    </Form.Item>
    <Button htmlType="submit" loading={isSubmitting} type="primary">{submitLabel}</Button>
  </Form>;
}
