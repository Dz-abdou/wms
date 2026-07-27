import { requestJson } from "../../../shared/api/apiClient";
import { receivingApiPaths } from "../receivingConstants";
import type {
  GoodsReceipt,
  GoodsReceiptCandidate,
  GoodsReceiptDetail,
  GoodsReceiptInput,
  GoodsReceiptListQuery,
  GoodsReceiptListResult,
} from "./receivingTypes";

export function getGoodsReceipts(
  query: GoodsReceiptListQuery,
  signal?: AbortSignal,
) {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  });
  if (query.purchaseOrderNumber?.trim()) {
    parameters.set("purchaseOrderNumber", query.purchaseOrderNumber.trim());
  }
  if (query.warehouseId) parameters.set("warehouseId", query.warehouseId);
  return requestJson<GoodsReceiptListResult>(
    `${receivingApiPaths.goodsReceipts}?${parameters}`,
    { signal },
  );
}

export function getGoodsReceipt(id: string, signal?: AbortSignal) {
  return requestJson<GoodsReceiptDetail>(
    receivingApiPaths.goodsReceiptById(id),
    {
      signal,
    },
  );
}

export function getGoodsReceiptCandidate(
  purchaseOrderId: string,
  signal?: AbortSignal,
) {
  return requestJson<GoodsReceiptCandidate>(
    receivingApiPaths.receiptCandidate(purchaseOrderId),
    { signal },
  );
}

export function createGoodsReceipt(input: GoodsReceiptInput) {
  return requestJson<GoodsReceipt>(receivingApiPaths.goodsReceipts, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
}
