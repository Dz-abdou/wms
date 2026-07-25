import { Alert, Button, Card, Empty, Form, Select, Spin, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useProducts } from "../../products/api/useProducts";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { useMovementHistory } from "../api/useInventory";
import type { InventoryMovement } from "../api/inventoryTypes";
import { inventoryPageSize, inventoryRoutes } from "../inventoryConstants";

type HistorySelection = {
  productId: string;
  warehouseId: string;
};

export function InventoryMovementHistoryPage() {
  const { i18n, t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [form] = Form.useForm<HistorySelection>();
  const [selection, setSelection] = useState<HistorySelection | undefined>(() => {
    const productId = searchParams.get("productId");
    const warehouseId = searchParams.get("warehouseId");
    return productId && warehouseId ? { productId, warehouseId } : undefined;
  });
  const products = useProducts({ page: 1, pageSize: inventoryPageSize });
  const warehouses = useWarehouses(1, inventoryPageSize);
  const movements = useMovementHistory(
    selection?.productId,
    selection?.warehouseId,
  );

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

  if (products.isLoading || warehouses.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("inventory.loadingSources")}
      />
    );
  }

  if (products.error || warehouses.error) {
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
  }

  return (
    <section>
      <div className="page-heading">
        <div>
          <Typography.Title level={2}>
            {t("inventory.historyTitle")}
          </Typography.Title>
          <Typography.Paragraph>
            {t("inventory.historySubtitle")}
          </Typography.Paragraph>
        </div>
        <Button
          onClick={() => navigate(inventoryRoutes.adjustments)}
          type="primary"
        >
          {t("inventory.newAdjustment")}
        </Button>
      </div>
      <Card>
        <Form<HistorySelection>
          form={form}
          initialValues={selection}
          layout="inline"
          onFinish={setSelection}
        >
          <Form.Item
            label={t("inventory.form.product")}
            name="productId"
            rules={[{ required: true, message: t("inventory.form.productRequired") }]}
          >
            <Select
              aria-label={t("inventory.form.product")}
              options={products.data?.items
                .filter((product) => product.isActive)
                .map((product) => ({
                  value: product.id,
                  label: `${product.sku} — ${product.name}`,
                }))}
              showSearch
              optionFilterProp="label"
            />
          </Form.Item>
          <Form.Item
            label={t("inventory.form.warehouse")}
            name="warehouseId"
            rules={[
              { required: true, message: t("inventory.form.warehouseRequired") },
            ]}
          >
            <Select
              aria-label={t("inventory.form.warehouse")}
              options={warehouses.data?.items
                .filter((warehouse) => warehouse.isActive)
                .map((warehouse) => ({
                  value: warehouse.id,
                  label: `${warehouse.code} — ${warehouse.name}`,
                }))}
              showSearch
              optionFilterProp="label"
            />
          </Form.Item>
          <Form.Item>
            <Button htmlType="submit" type="primary">
              {t("inventory.viewHistory")}
            </Button>
          </Form.Item>
        </Form>
      </Card>
      <Card className="inventory-history-card" title={t("inventory.historyTitle")}>
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
        ) : movements.data?.items.length === 0 ? (
          <Empty description={t("inventory.emptyHistory")} />
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
