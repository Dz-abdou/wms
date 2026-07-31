import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import {
  ListFilter,
  ListPageLayout,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { useGoodsReceipts } from "../api/useReceiving";
import type { GoodsReceiptListItem } from "../api/receivingTypes";
import { receivingRoutes } from "../receivingConstants";

export function GoodsReceiptListPage() {
  const { i18n, t } = useTranslation();
  const listQuery = useUrlListQuery();
  const [warehouseSearch, setWarehouseSearch] = useState("");
  const receipts = useGoodsReceipts({
    ...listQuery.request,
    purchaseOrderNumber: listQuery.get("purchaseOrderNumber"),
    warehouseId: listQuery.get("warehouseId"),
  });
  const warehouses = useWarehouses({
    page: 1,
    pageSize: 20,
    search: warehouseSearch,
  });
  const columns: ColumnsType<GoodsReceiptListItem> = [
    {
      title: t("receiving.number"),
      dataIndex: "number",
      key: "number",
    },
    {
      title: t("receiving.purchaseOrder"),
      dataIndex: "purchaseOrderNumber",
      key: "purchaseOrderNumber",
    },
    {
      title: t("receiving.warehouse"),
      key: "warehouse",
      render: (_, receipt) =>
        `${receipt.warehouseCode} — ${receipt.warehouseName}`,
    },
    {
      title: t("receiving.receivedAt"),
      dataIndex: "receivedAtUtc",
      key: "receivedAtUtc",
      render: (value: string) =>
        formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)),
    },
    {
      title: t("receiving.lineCount"),
      dataIndex: "lineCount",
      key: "lineCount",
    },
    {
      title: t("receiving.actions"),
      key: "actions",
      fixed: "right",
      width: 120,
      render: (_, receipt) => (
        <ReturnAwareLink to={receivingRoutes.detail(receipt.id)}>
          {t("receiving.view")}
        </ReturnAwareLink>
      ),
    },
  ];

  return (
    <ListPageLayout
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <>
          <ListFilter label={t("receiving.purchaseOrder")} width="regular">
            <Input
              allowClear
              aria-label={t("receiving.purchaseOrder")}
              defaultValue={listQuery.get("purchaseOrderNumber")}
              key={listQuery.get("purchaseOrderNumber") ?? "purchaseOrder"}
              onPressEnter={(event) =>
                listQuery.update({
                  purchaseOrderNumber: event.currentTarget.value || undefined,
                })
              }
              placeholder={t("receiving.purchaseOrderPlaceholder")}
            />
          </ListFilter>
          <ListFilter label={t("receiving.warehouse")} width="regular">
            <Select
              allowClear
              aria-label={t("receiving.warehouse")}
              filterOption={false}
              onChange={(value) => listQuery.update({ warehouseId: value })}
              onSearch={setWarehouseSearch}
              options={(warehouses.data?.items ?? []).map((warehouse) => ({
                value: warehouse.id,
                label: `${warehouse.code} — ${warehouse.name}`,
              }))}
              placeholder={t("receiving.warehouse")}
              showSearch
              value={listQuery.get("warehouseId")}
            />
          </ListFilter>
        </>
      }
      subtitle={t("receiving.listSubtitle")}
      title={t("receiving.title")}
    >
      {receipts.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("receiving.loadingList")}
        />
      ) : receipts.error ? (
        <Alert
          message={getErrorMessage(
            t,
            receipts.error,
            "receiving.errors.loadList",
          )}
          showIcon
          type="error"
        />
      ) : receipts.data?.items.length === 0 ? (
        <Empty description={t("receiving.empty")} />
      ) : (
        <Table
          columns={columns}
          dataSource={receipts.data?.items}
          loading={receipts.isFetching}
          pagination={
            receipts.data ? listQuery.toTablePagination(receipts.data) : false
          }
          rowKey="id"
          scroll={{ x: 900 }}
        />
      )}
    </ListPageLayout>
  );
}
