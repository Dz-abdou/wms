import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { QuantityDelta } from "../../../shared/components/QuantityDelta";
import { useProducts } from "../../products/api/useProducts";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { useMovementHistory } from "../api/useInventory";
import type {
  InventoryMovement,
  InventoryMovementFilter,
} from "../api/inventoryTypes";
import { inventoryPageSize, inventoryRoutes } from "../inventoryConstants";
import { receivingRoutes } from "../../receiving/receivingConstants";
import {
  ListFilter,
  ListPageLayout,
  ReturnAwareLink,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";

export function InventoryMovementHistoryPage() {
  const { i18n, t } = useTranslation();
  const listQuery = useUrlListQuery();
  const [productSearch, setProductSearch] = useState("");
  const [warehouseSearch, setWarehouseSearch] = useState("");
  const products = useProducts({
    page: 1,
    pageSize: inventoryPageSize,
    search: productSearch,
  });
  const warehouses = useWarehouses({
    page: 1,
    pageSize: inventoryPageSize,
    search: warehouseSearch,
  });
  const movements = useMovementHistory({
    ...listQuery.request,
    productId: listQuery.get("productId"),
    warehouseId: listQuery.get("warehouseId"),
    type: listQuery.get("type") as InventoryMovementFilter["type"],
    reference: listQuery.get("reference"),
    fromUtc: listQuery.get("fromUtc"),
    toUtc: listQuery.get("toUtc"),
  });
  const columns: ColumnsType<InventoryMovement> = [
    {
      title: t("inventory.table.product"),
      key: "product",
      width: 250,
      render: (_, x) => `${x.productSku} — ${x.productName}`,
    },
    {
      title: t("inventory.table.warehouse"),
      key: "warehouse",
      width: 230,
      render: (_, x) => `${x.warehouseCode} — ${x.warehouseName}`,
    },
    {
      title: t("inventory.table.type"),
      dataIndex: "type",
      width: 150,
      render: (value) =>
        t(
          value === "ManualIncrease"
            ? "inventory.types.increase"
            : value === "ManualDecrease"
              ? "inventory.types.decrease"
              : value === "CycleCountIncrease"
                ? "inventory.types.cycleCountIncrease"
                : value === "CycleCountDecrease"
                  ? "inventory.types.cycleCountDecrease"
                  : "inventory.types.goodsReceipt",
        ),
    },
    {
      title: t("inventory.table.delta"),
      dataIndex: "quantityDeltaInUnit",
      width: 120,
      render: (value) => <QuantityDelta value={value} />,
    },
    {
      title: t("inventory.form.unitOfMeasure"),
      dataIndex: "unitOfMeasure",
      width: 100,
    },
    {
      title: t("inventory.table.balanceAfter"),
      dataIndex: "balanceAfter",
      width: 150,
    },
    {
      title: t("inventory.table.reference"),
      key: "reference",
      width: 170,
      render: (_, x) =>
        x.inventoryAdjustmentId ? (
          <ReturnAwareLink
            to={inventoryRoutes.adjustmentDetail(x.inventoryAdjustmentId)}
          >
            {x.adjustmentReference ?? t("inventory.adjustment")}
          </ReturnAwareLink>
        ) : x.goodsReceiptId ? (
          <ReturnAwareLink to={receivingRoutes.detail(x.goodsReceiptId)}>
            {x.goodsReceiptNumber ?? t("receiving.title")}
          </ReturnAwareLink>
        ) : x.cycleCountId ? (
          <ReturnAwareLink
            to={inventoryRoutes.cycleCountDetail(x.cycleCountId)}
          >
            {x.cycleCountReference ?? t("inventory.cycleCounts.document")}
          </ReturnAwareLink>
        ) : (
          "—"
        ),
    },
    {
      title: t("inventory.table.created"),
      dataIndex: "createdAtUtc",
      width: 190,
      render: (value) =>
        formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)),
    },
  ];
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
    <ListPageLayout
      actions={
        <RouteActionButton to={inventoryRoutes.adjustmentCreate}>
          {t("inventory.recordAdjustment")}
        </RouteActionButton>
      }
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <>
          <ListFilter label={t("inventory.form.product")} width="regular">
            <Select
              allowClear
              aria-label={t("inventory.form.product")}
              filterOption={false}
              onChange={(value) => listQuery.update({ productId: value })}
              onSearch={setProductSearch}
              options={products.data?.items
                .filter((x) => x.isActive)
                .map((x) => ({ value: x.id, label: `${x.sku} — ${x.name}` }))}
              placeholder={t("inventory.form.product")}
              showSearch
              value={listQuery.get("productId")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.form.warehouse")} width="regular">
            <Select
              allowClear
              aria-label={t("inventory.form.warehouse")}
              filterOption={false}
              onChange={(value) => listQuery.update({ warehouseId: value })}
              onSearch={setWarehouseSearch}
              options={warehouses.data?.items
                .filter((x) => x.isActive)
                .map((x) => ({ value: x.id, label: `${x.code} — ${x.name}` }))}
              placeholder={t("inventory.form.warehouse")}
              showSearch
              value={listQuery.get("warehouseId")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.table.type")} width="compact">
            <Select
              allowClear
              aria-label={t("inventory.table.type")}
              onChange={(value) => listQuery.update({ type: value })}
              options={[
                {
                  value: "ManualIncrease",
                  label: t("inventory.types.increase"),
                },
                {
                  value: "ManualDecrease",
                  label: t("inventory.types.decrease"),
                },
                {
                  value: "GoodsReceipt",
                  label: t("inventory.types.goodsReceipt"),
                },
                {
                  value: "CycleCountIncrease",
                  label: t("inventory.types.cycleCountIncrease"),
                },
                {
                  value: "CycleCountDecrease",
                  label: t("inventory.types.cycleCountDecrease"),
                },
              ]}
              placeholder={t("inventory.table.type")}
              value={listQuery.get("type")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.table.reference")} width="regular">
            <Input
              allowClear
              aria-label={t("inventory.table.reference")}
              defaultValue={listQuery.get("reference")}
              key={listQuery.get("reference") ?? "reference"}
              onPressEnter={(event) =>
                listQuery.update({ reference: event.currentTarget.value })
              }
              placeholder={t("inventory.table.reference")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.filters.fromDate")} width="compact">
            <Input
              aria-label={t("inventory.filters.fromDate")}
              onChange={(event) =>
                listQuery.update({
                  fromUtc: event.target.value
                    ? `${event.target.value}T00:00:00.000Z`
                    : undefined,
                })
              }
              type="date"
              value={listQuery.get("fromUtc")?.slice(0, 10)}
            />
          </ListFilter>
          <ListFilter label={t("inventory.filters.toDate")} width="compact">
            <Input
              aria-label={t("inventory.filters.toDate")}
              onChange={(event) =>
                listQuery.update({
                  toUtc: event.target.value
                    ? `${event.target.value}T23:59:59.999Z`
                    : undefined,
                })
              }
              type="date"
              value={listQuery.get("toUtc")?.slice(0, 10)}
            />
          </ListFilter>
        </>
      }
      subtitle={t("inventory.historySubtitle")}
      title={t("inventory.historyTitle")}
    >
      {movements.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("inventory.loadingHistory")}
        />
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
          columns={columns}
          dataSource={movements.data?.items}
          loading={movements.isFetching}
          pagination={
            movements.data ? listQuery.toTablePagination(movements.data) : false
          }
          rowKey="id"
          scroll={{ x: 1360 }}
        />
      )}
    </ListPageLayout>
  );
}
