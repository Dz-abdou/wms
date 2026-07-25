import { Alert, Button, Card, Empty, Form, Input, Select, Spin, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useListPagination } from "../../../shared/pagination/pagination";
import { useProducts } from "../../products/api/useProducts";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { useMovementHistory } from "../api/useInventory";
import type { InventoryMovement, InventoryMovementFilter } from "../api/inventoryTypes";
import { inventoryPageSize, inventoryRoutes } from "../inventoryConstants";

type MovementFilters = Pick<InventoryMovementFilter, "productId" | "warehouseId" | "type" | "reference">;

export function InventoryMovementHistoryPage() {
  const { i18n, t } = useTranslation(); const navigate = useNavigate(); const pagination = useListPagination(); const [filters, setFilters] = useState<MovementFilters>({});
  const products = useProducts({ page: 1, pageSize: inventoryPageSize }); const warehouses = useWarehouses(1, inventoryPageSize);
  const movements = useMovementHistory({ ...pagination.request, ...filters });
  const columns: ColumnsType<InventoryMovement> = [
    { title: t("inventory.table.product"), key: "product", render: (_, x) => `${x.productSku} — ${x.productName}` },
    { title: t("inventory.table.warehouse"), key: "warehouse", render: (_, x) => `${x.warehouseCode} — ${x.warehouseName}` },
    { title: t("inventory.table.type"), dataIndex: "type", render: (value) => t(value === "ManualIncrease" ? "inventory.types.increase" : "inventory.types.decrease") },
    { title: t("inventory.table.delta"), key: "delta", render: (_, x) => `${x.quantityDeltaInUnit} ${x.unitOfMeasure}` },
    { title: t("inventory.table.balanceAfter"), dataIndex: "balanceAfter" },
    { title: t("inventory.table.reference"), key: "reference", render: (_, x) => x.inventoryAdjustmentId ? <Link to={inventoryRoutes.adjustmentDetail(x.inventoryAdjustmentId)}>{x.adjustmentReference ?? t("inventory.adjustment")}</Link> : "—" },
    { title: t("inventory.table.created"), dataIndex: "createdAtUtc", render: (value) => formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)) },
  ];
  if (products.isLoading || warehouses.isLoading) return <Spin className="page-spinner" size="large" tip={t("inventory.loadingSources")} />;
  if (products.error || warehouses.error) return <Alert message={getErrorMessage(t, products.error ?? warehouses.error, "inventory.errors.loadSources")} showIcon type="error" />;
  return <section><div className="page-heading"><div><Typography.Title level={2}>{t("inventory.historyTitle")}</Typography.Title><Typography.Paragraph>{t("inventory.historySubtitle")}</Typography.Paragraph></div><Button onClick={() => navigate(inventoryRoutes.adjustmentCreate)} type="primary">{t("inventory.newAdjustment")}</Button></div><Card><Form<MovementFilters> layout="inline" onFinish={(values) => { setFilters(values); pagination.resetPage(); }}><Form.Item label={t("inventory.form.product")} name="productId"><Select allowClear aria-label={t("inventory.form.product")} options={products.data?.items.filter(x => x.isActive).map(x => ({ value: x.id, label: `${x.sku} — ${x.name}` }))} showSearch optionFilterProp="label" /></Form.Item><Form.Item label={t("inventory.form.warehouse")} name="warehouseId"><Select allowClear aria-label={t("inventory.form.warehouse")} options={warehouses.data?.items.filter(x => x.isActive).map(x => ({ value: x.id, label: `${x.code} — ${x.name}` }))} showSearch optionFilterProp="label" /></Form.Item><Form.Item label={t("inventory.table.type")} name="type"><Select allowClear options={[{ value: "ManualIncrease", label: t("inventory.types.increase") }, { value: "ManualDecrease", label: t("inventory.types.decrease") }]} /></Form.Item><Form.Item label={t("inventory.table.reference")} name="reference"><Input /></Form.Item><Form.Item><Button htmlType="submit" type="primary">{t("inventory.applyFilters")}</Button></Form.Item></Form></Card><Card className="inventory-history-card" title={t("inventory.historyTitle")}>{movements.isLoading ? <Spin tip={t("inventory.loadingHistory")} /> : movements.error ? <Alert message={getErrorMessage(t, movements.error, "inventory.errors.loadHistory")} showIcon type="error" /> : movements.data?.items.length === 0 ? <Empty description={t("inventory.emptyHistory")} /> : <Table columns={columns} dataSource={movements.data?.items} loading={movements.isFetching} pagination={movements.data ? pagination.toTablePagination(movements.data) : false} rowKey="id" />}</Card></section>;
}
