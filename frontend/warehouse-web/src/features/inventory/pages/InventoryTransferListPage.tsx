import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import {
  ListFilter,
  ListPageLayout,
  NewPageAction,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { useTransfers } from "../api/useInventory";
import type { InventoryTransferListItem } from "../api/inventoryTypes";
import { inventoryPageSize, inventoryRoutes } from "../inventoryConstants";

export function InventoryTransferListPage() {
  const { i18n, t } = useTranslation();
  const listQuery = useUrlListQuery();
  const warehouses = useWarehouses({ page: 1, pageSize: inventoryPageSize });
  const transfers = useTransfers({
    ...listQuery.request,
    sourceWarehouseId: listQuery.get("sourceWarehouseId"),
    destinationWarehouseId: listQuery.get("destinationWarehouseId"),
    reference: listQuery.get("reference"),
    fromUtc: listQuery.get("fromUtc"),
    toUtc: listQuery.get("toUtc"),
  });
  const columns: ColumnsType<InventoryTransferListItem> = [
    {
      title: t("inventory.transfers.sourceWarehouse"),
      key: "sourceWarehouse",
      render: (_, item) =>
        `${item.sourceWarehouseCode} — ${item.sourceWarehouseName}`,
    },
    {
      title: t("inventory.transfers.destinationWarehouse"),
      key: "destinationWarehouse",
      render: (_, item) =>
        `${item.destinationWarehouseCode} — ${item.destinationWarehouseName}`,
    },
    {
      title: t("inventory.table.reference"),
      dataIndex: "reference",
      render: (value) => value ?? "—",
    },
    { title: t("inventory.table.lines"), dataIndex: "lineCount" },
    {
      title: t("inventory.transfers.transferredAt"),
      dataIndex: "transferredAtUtc",
      render: (value) =>
        formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)),
    },
    {
      title: t("inventory.table.actions"),
      key: "actions",
      fixed: "right",
      width: 120,
      render: (_, item) => (
        <ReturnAwareLink to={inventoryRoutes.transferDetail(item.id)}>
          {t("inventory.view")}
        </ReturnAwareLink>
      ),
    },
  ];

  const warehouseOptions = warehouses.data?.items
    .filter((warehouse) => warehouse.isActive)
    .map((warehouse) => ({
      value: warehouse.id,
      label: `${warehouse.code} — ${warehouse.name}`,
    }));

  return (
    <ListPageLayout
      actions={<NewPageAction to={inventoryRoutes.transferCreate} />}
      filters={
        <>
          <ListFilter
            label={t("inventory.transfers.sourceWarehouse")}
            width="regular"
          >
            <Select
              allowClear
              aria-label={t("inventory.transfers.sourceWarehouse")}
              onChange={(value) =>
                listQuery.update({ sourceWarehouseId: value })
              }
              options={warehouseOptions}
              optionFilterProp="label"
              placeholder={t("inventory.transfers.sourceWarehouse")}
              showSearch
              value={listQuery.get("sourceWarehouseId")}
            />
          </ListFilter>
          <ListFilter
            label={t("inventory.transfers.destinationWarehouse")}
            width="regular"
          >
            <Select
              allowClear
              aria-label={t("inventory.transfers.destinationWarehouse")}
              onChange={(value) =>
                listQuery.update({ destinationWarehouseId: value })
              }
              options={warehouseOptions}
              optionFilterProp="label"
              placeholder={t("inventory.transfers.destinationWarehouse")}
              showSearch
              value={listQuery.get("destinationWarehouseId")}
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
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      subtitle={t("inventory.transfers.subtitle")}
      title={t("inventory.transfers.title")}
    >
      {transfers.isLoading || warehouses.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("inventory.transfers.loading")}
        />
      ) : transfers.error || warehouses.error ? (
        <Alert
          message={getErrorMessage(
            t,
            transfers.error ?? warehouses.error,
            "inventory.errors.loadTransfers",
          )}
          showIcon
          type="error"
        />
      ) : transfers.data?.items.length === 0 ? (
        <Empty description={t("inventory.transfers.empty")} />
      ) : (
        <Table
          columns={columns}
          dataSource={transfers.data?.items}
          loading={transfers.isFetching}
          pagination={
            transfers.data ? listQuery.toTablePagination(transfers.data) : false
          }
          rowKey="id"
          scroll={{ x: 1120 }}
        />
      )}
    </ListPageLayout>
  );
}
