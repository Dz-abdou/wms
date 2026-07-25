import {
  Alert,
  Button,
  Card,
  Empty,
  Form,
  Input,
  InputNumber,
  Radio,
  Select,
  Spin,
  Table,
  Typography,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import {
  EditableFormListTable,
  type EditableFormListTableRow,
} from "../../../shared/components/EditableFormListTable";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useProducts } from "../../products/api/useProducts";
import type { Product } from "../../products/api/productTypes";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { useAdjustInventory, useMovementHistory } from "../api/useInventory";
import type {
  InventoryAdjustmentInput,
  InventoryAdjustmentLineInput,
  InventoryMovement,
} from "../api/inventoryTypes";
import {
  fractionalBaseUnitCodes,
  inventoryPageSize,
} from "../inventoryConstants";

type AdjustmentRow = object;
type Selection = Pick<
  InventoryAdjustmentLineInput,
  "productId" | "warehouseId"
>;

export function InventoryPage() {
  const { i18n, t } = useTranslation();
  const [form] = Form.useForm<InventoryAdjustmentInput>();
  const products = useProducts({ page: 1, pageSize: inventoryPageSize });
  const warehouses = useWarehouses(1, inventoryPageSize);
  const feedback = useApiFeedback();
  const adjustment = useAdjustInventory();
  const lines = Form.useWatch("lines", form) ?? [];
  const [selection, setSelection] = useState<Selection>();
  const movements = useMovementHistory(
    selection?.productId,
    selection?.warehouseId,
  );
  const productOptions = products.data?.items
    .filter((product) => product.isActive)
    .map((product) => ({
      value: product.id,
      label: `${product.sku} — ${product.name}`,
    }));
  const warehouseOptions = warehouses.data?.items
    .filter((warehouse) => warehouse.isActive)
    .map((warehouse) => ({
      value: warehouse.id,
      label: `${warehouse.code} — ${warehouse.name}`,
    }));
  const lineColumns = (
    remove: (fieldName: number) => void,
  ): ColumnsType<AdjustmentRow & EditableFormListTableRow> => [
    {
      title: t("inventory.form.product"),
      key: "product",
      width: 250,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "productId"]}
          rules={[
            { required: true, message: t("inventory.form.productRequired") },
          ]}
          style={{ marginBottom: 0 }}
        >
          <Select
            aria-label={t("inventory.form.product")}
            options={productOptions}
            showSearch
            optionFilterProp="label"
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.form.warehouse"),
      key: "warehouse",
      width: 220,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "warehouseId"]}
          rules={[
            { required: true, message: t("inventory.form.warehouseRequired") },
          ]}
          style={{ marginBottom: 0 }}
        >
          <Select
            aria-label={t("inventory.form.warehouse")}
            options={warehouseOptions}
            showSearch
            optionFilterProp="label"
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.form.direction"),
      key: "direction",
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "direction"]}
          initialValue="Increase"
          style={{ marginBottom: 0 }}
        >
          <Radio.Group
            options={[
              { value: "Increase", label: t("inventory.types.increase") },
              { value: "Decrease", label: t("inventory.types.decrease") },
            ]}
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.form.unitOfMeasure"),
      key: "unit",
      render: (_, row) => {
        const product = products.data?.items.find(
          (item) => item.id === lines[row.fieldName]?.productId,
        );
        return (
          <Form.Item
            name={[row.fieldName, "unitOfMeasure"]}
            rules={[
              {
                required: true,
                message: t("inventory.form.unitOfMeasureRequired"),
              },
            ]}
            style={{ marginBottom: 0 }}
          >
            <Select
              aria-label={t("inventory.form.unitOfMeasure")}
              disabled={!product}
              options={unitOptions(product).map((unit) => ({
                value: unit.value,
                label: unit.value,
              }))}
            />
          </Form.Item>
        );
      },
    },
    {
      title: t("inventory.form.quantity"),
      key: "quantity",
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "quantity"]}
          rules={[
            { required: true, message: t("inventory.form.quantityRequired") },
          ]}
          style={{ marginBottom: 0 }}
        >
          <InputNumber
            aria-label={t("inventory.form.quantity")}
            min={0.001}
            precision={3}
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.table.actions"),
      key: "actions",
      render: (_, row) => (
        <Button danger onClick={() => remove(row.fieldName)} type="text">
          {t("inventory.removeLine")}
        </Button>
      ),
    },
  ];
  const movementColumns: ColumnsType<InventoryMovement> = [
    {
      title: t("inventory.table.type"),
      dataIndex: "type",
      render: (type) =>
        t(
          type === "ManualIncrease"
            ? "inventory.types.increase"
            : "inventory.types.decrease",
        ),
    },
    {
      title: t("inventory.table.delta"),
      dataIndex: "quantityDeltaInUnit",
      render: (value, movement) => `${value} ${movement.unitOfMeasure}`,
    },
    { title: t("inventory.table.balanceAfter"), dataIndex: "balanceAfter" },
    {
      title: t("inventory.table.created"),
      dataIndex: "createdAtUtc",
      render: (value) =>
        formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)),
    },
  ];
  async function submit(input: InventoryAdjustmentInput) {
    try {
      const result = await adjustment.mutateAsync(input);
      const firstLine = result.lines[0];
      if (firstLine) setSelection(firstLine);
      form.resetFields(["lines"]);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, "inventory.errors.adjust");
    }
  }
  if (products.isLoading || warehouses.isLoading)
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("inventory.loadingSources")}
      />
    );
  if (products.error || warehouses.error)
    return (
      <Alert
        message={getErrorMessage(
          t,
          products.error ?? warehouses.error,
          "inventory.errors.loadSources",
        )}
        showIcon
        type="error"
      />
    );
  return (
    <section>
      <div className="page-heading">
        <Typography.Title level={2}>{t("inventory.title")}</Typography.Title>
        <Typography.Paragraph>{t("inventory.subtitle")}</Typography.Paragraph>
      </div>
      <Card title={t("inventory.adjustTitle")}>
        <Form
          form={form}
          initialValues={{ reason: "StockCorrection", lines: [] }}
          layout="vertical"
          onFinish={submit}
          onValuesChange={(changedValues) => {
            const changedLines = changedValues.lines as
              Array<Partial<InventoryAdjustmentLineInput>> | undefined;
            changedLines?.forEach((line, index) => {
              if (!line?.productId) return;
              const product = products.data?.items.find(
                (candidate) => candidate.id === line.productId,
              );
              if (product)
                form.setFieldValue(
                  ["lines", index, "unitOfMeasure"],
                  product.baseUnitOfMeasure,
                );
            });
          }}
        >
          <Form.Item
            label={t("inventory.form.reason")}
            name="reason"
            rules={[{ required: true }]}
          >
            <Select
              options={[
                "StockCorrection",
                "Damage",
                "WriteOff",
                "FoundStock",
                "InitialBalance",
              ].map((value) => ({
                value,
                label: t(`inventory.reasons.${value}`),
              }))}
            />
          </Form.Item>
          <Form.Item label={t("inventory.form.reference")} name="reference">
            <Input maxLength={100} />
          </Form.Item>
          <Form.Item label={t("inventory.form.note")} name="note">
            <Input.TextArea maxLength={1000} />
          </Form.Item>
          <EditableFormListTable<AdjustmentRow>
            addInitialValue={{ direction: "Increase" }}
            addLabel={t("inventory.addLine")}
            columns={lineColumns}
            createRow={() => ({})}
            name="lines"
            scroll={{ x: 1200 }}
          />
          <Button
            htmlType="submit"
            loading={adjustment.isPending}
            type="primary"
          >
            {t("inventory.adjust")}
          </Button>
        </Form>
      </Card>
      <Card title={t("inventory.historyTitle")}>
        {!selection ? (
          <Empty description={t("inventory.selectForHistory")} />
        ) : movements.isLoading ? (
          <Spin tip={t("inventory.loadingHistory")} />
        ) : movements.error ? (
          <Alert
            message={getErrorMessage(
              t,
              movements.error,
              "inventory.errors.loadHistory",
            )}
            showIcon
            type="error"
          />
        ) : (
          <Table
            columns={movementColumns}
            dataSource={movements.data?.items}
            pagination={false}
            rowKey="id"
          />
        )}
      </Card>
    </section>
  );
}

function unitOptions(product: Product | undefined) {
  if (!product) return [];
  return [
    { value: product.baseUnitOfMeasure },
    ...product.unitConversions.map((conversion) => ({
      value: conversion.unitOfMeasure,
    })),
  ].filter((unit) => fractionalBaseUnitCodes.has(unit.value) || unit.value);
}
