import { Alert, Empty, Input, Select, Spin, Table, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useProductCategories } from "../../products/api/useProductCategories";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import {
  ListFilter,
  ListPageLayout,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useInventoryOverview } from "../api/useInventory";
import type { InventoryOverviewItem } from "../api/inventoryTypes";
import { inventoryPageSize, inventoryRoutes } from "../inventoryConstants";

export function InventoryOverviewPage() {
  const { i18n, t } = useTranslation();
  const listQuery = useUrlListQuery();
  const [warehouseSearch, setWarehouseSearch] = useState("");
  const [categorySearch, setCategorySearch] = useState("");
  const warehouses = useWarehouses({
    page: 1,
    pageSize: inventoryPageSize,
    search: warehouseSearch,
  });
  const categories = useProductCategories({
    page: 1,
    pageSize: inventoryPageSize,
    search: categorySearch,
  });
  const activeValue = listQuery.get("isActive");
  const overview = useInventoryOverview({
    ...listQuery.request,
    search: listQuery.get("search"),
    warehouseId: listQuery.get("warehouseId"),
    categoryId: listQuery.get("categoryId"),
    isActive: activeValue === undefined ? undefined : activeValue === "true",
  });
  const columns: ColumnsType<InventoryOverviewItem> = [
    {
      title: t("inventory.table.product"),
      key: "product",
      width: 290,
      render: (_, item) => `${item.productSku} — ${item.productName}`,
    },
    {
      title: t("inventory.table.warehouse"),
      key: "warehouse",
      width: 260,
      render: (_, item) => `${item.warehouseCode} — ${item.warehouseName}`,
    },
    {
      title: t("inventory.table.onHand"),
      key: "quantity",
      width: 160,
      render: (_, item) => `${item.quantity} ${item.baseUnitOfMeasure}`,
    },
    {
      title: t("inventory.table.status"),
      dataIndex: "productIsActive",
      width: 120,
      render: (isActive) => (
        <Tag color={isActive ? "green" : "default"}>
          {t(isActive ? "products.status.active" : "products.status.inactive")}
        </Tag>
      ),
    },
    {
      title: t("inventory.table.updated"),
      dataIndex: "updatedAtUtc",
      width: 190,
      render: (value) =>
        formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)),
    },
    {
      title: t("inventory.table.actions"),
      key: "actions",
      fixed: "right",
      width: 130,
      render: (_, item) => (
        <ReturnAwareLink
          to={`${inventoryRoutes.movementHistory}?productId=${item.productId}&warehouseId=${item.warehouseId}`}
        >
          {t("inventory.viewHistory")}
        </ReturnAwareLink>
      ),
    },
  ];

  if (warehouses.isLoading || categories.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("inventory.loadingSources")}
      />
    );
  }

  if (warehouses.error || categories.error) {
    return (
      <Alert
        message={getErrorMessage(
          t,
          warehouses.error ?? categories.error,
          "inventory.errors.loadSources",
        )}
        showIcon
        type="error"
      />
    );
  }

  return (
    <ListPageLayout
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <>
          <ListFilter label={t("inventory.filters.search")} width="search">
            <Input.Search
              allowClear
              aria-label={t("inventory.filters.search")}
              defaultValue={listQuery.get("search")}
              key={listQuery.get("search") ?? "search"}
              onSearch={(value) => listQuery.update({ search: value })}
              placeholder={t("inventory.filters.search")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.form.warehouse")} width="regular">
            <Select
              allowClear
              aria-label={t("inventory.form.warehouse")}
              filterOption={false}
              onChange={(value) => listQuery.update({ warehouseId: value })}
              onSearch={setWarehouseSearch}
              options={warehouses.data?.items.map((warehouse) => ({
                value: warehouse.id,
                label: `${warehouse.code} — ${warehouse.name}`,
              }))}
              placeholder={t("inventory.form.warehouse")}
              showSearch
              value={listQuery.get("warehouseId")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.filters.category")} width="regular">
            <Select
              allowClear
              aria-label={t("inventory.filters.category")}
              filterOption={false}
              onChange={(value) => listQuery.update({ categoryId: value })}
              onSearch={setCategorySearch}
              options={categories.data?.items.map((category) => ({
                value: category.id,
                label: `${category.code} — ${category.name}`,
              }))}
              placeholder={t("inventory.filters.category")}
              showSearch
              value={listQuery.get("categoryId")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.filters.status")} width="compact">
            <Select
              allowClear
              aria-label={t("inventory.filters.status")}
              onChange={(value) => listQuery.update({ isActive: value })}
              options={[
                { value: "true", label: t("products.status.active") },
                { value: "false", label: t("products.status.inactive") },
              ]}
              placeholder={t("inventory.filters.status")}
              value={activeValue}
            />
          </ListFilter>
        </>
      }
      subtitle={t("inventory.overviewSubtitle")}
      title={t("inventory.overviewTitle")}
    >
      {overview.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("inventory.loadingOverview")}
        />
      ) : overview.error ? (
        <Alert
          message={getErrorMessage(
            t,
            overview.error,
            "inventory.errors.loadOverview",
          )}
          showIcon
          type="error"
        />
      ) : overview.data?.items.length === 0 ? (
        <Empty description={t("inventory.emptyOverview")} />
      ) : (
        <Table
          columns={columns}
          dataSource={overview.data?.items}
          loading={overview.isFetching}
          pagination={
            overview.data ? listQuery.toTablePagination(overview.data) : false
          }
          rowKey={(item) => `${item.productId}-${item.warehouseId}`}
          scroll={{ x: 1150 }}
        />
      )}
    </ListPageLayout>
  );
}
