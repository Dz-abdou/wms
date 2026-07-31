import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";
import { ApiError } from "../../../shared/api/apiClient";
import { GoodsReceiptListPage } from "../pages/GoodsReceiptListPage";

const { useReceiptsMock, useWarehousesMock } = vi.hoisted(() => ({
  useReceiptsMock: vi.fn(),
  useWarehousesMock: vi.fn(),
}));

vi.mock("../api/useReceiving", () => ({
  useGoodsReceipts: useReceiptsMock,
}));
vi.mock("../../warehouses/api/useWarehouses", () => ({
  useWarehouses: useWarehousesMock,
}));

describe("GoodsReceiptListPage", () => {
  beforeEach(() => {
    useReceiptsMock.mockReset();
    useWarehousesMock.mockReturnValue({ data: { items: [] } });
  });

  it("renders loading, error, empty, and populated states", () => {
    useReceiptsMock.mockReturnValue({
      data: undefined,
      error: null,
      isFetching: true,
      isLoading: true,
    });
    const { rerender } = renderPage();
    expect(screen.getByText("Loading goods receipts…")).toBeInTheDocument();

    useReceiptsMock.mockReturnValue({
      data: undefined,
      error: new ApiError(500, {}),
      isFetching: false,
      isLoading: false,
    });
    rerender(
      <MemoryRouter>
        <GoodsReceiptListPage />
      </MemoryRouter>,
    );
    expect(
      screen.getByText("Goods receipts could not be loaded."),
    ).toBeInTheDocument();

    useReceiptsMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
      error: null,
      isFetching: false,
      isLoading: false,
    });
    rerender(
      <MemoryRouter>
        <GoodsReceiptListPage />
      </MemoryRouter>,
    );
    expect(
      screen.getByText("No goods receipts match these filters."),
    ).toBeInTheDocument();

    useReceiptsMock.mockReturnValue({
      data: {
        items: [
          {
            id: "receipt-1",
            number: "GR-2026-000001",
            purchaseOrderId: "order-1",
            purchaseOrderNumber: "PO-2026-000001",
            warehouseId: "warehouse-1",
            warehouseCode: "MAIN",
            warehouseName: "Main warehouse",
            receivedAtUtc: "2026-07-27T10:00:00Z",
            lineCount: 2,
          },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      },
      error: null,
      isFetching: false,
      isLoading: false,
    });
    rerender(
      <MemoryRouter>
        <GoodsReceiptListPage />
      </MemoryRouter>,
    );
    expect(screen.getByText("GR-2026-000001")).toBeInTheDocument();
    expect(screen.getByText("MAIN — Main warehouse")).toBeInTheDocument();
  });
});

function renderPage() {
  return render(
    <MemoryRouter>
      <GoodsReceiptListPage />
    </MemoryRouter>,
  );
}
