import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";
import { ApiError } from "../../../shared/api/apiClient";
import { PurchaseOrderListPage } from "./PurchaseOrderListPage";

const { usePurchaseOrdersMock, useSuppliersMock, useWarehousesMock } =
  vi.hoisted(() => ({
    usePurchaseOrdersMock: vi.fn(),
    useSuppliersMock: vi.fn(),
    useWarehousesMock: vi.fn(),
  }));

vi.mock("../api/usePurchasing", () => ({
  usePurchaseOrders: usePurchaseOrdersMock,
}));
vi.mock("../../suppliers/api/useSuppliers", () => ({
  useSuppliers: useSuppliersMock,
}));
vi.mock("../../warehouses/api/useWarehouses", () => ({
  useWarehouses: useWarehousesMock,
}));

describe("PurchaseOrderListPage", () => {
  beforeEach(() => {
    usePurchaseOrdersMock.mockReset();
    useSuppliersMock.mockReturnValue({ data: { items: [] } });
    useWarehousesMock.mockReturnValue({ data: { items: [] } });
  });

  it("renders loading, empty, error, and populated states", () => {
    usePurchaseOrdersMock.mockReturnValue({
      data: undefined,
      error: null,
      isFetching: true,
      isLoading: true,
    });
    const { rerender } = renderPage();
    expect(screen.getByText("Loading purchase orders…")).toBeInTheDocument();

    usePurchaseOrdersMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
      error: null,
      isFetching: false,
      isLoading: false,
    });
    rerender(
      <MemoryRouter>
        <PurchaseOrderListPage />
      </MemoryRouter>,
    );
    expect(
      screen.getByText("No purchase orders exist yet."),
    ).toBeInTheDocument();

    usePurchaseOrdersMock.mockReturnValue({
      data: undefined,
      error: new ApiError(500, {}),
      isFetching: false,
      isLoading: false,
    });
    rerender(
      <MemoryRouter>
        <PurchaseOrderListPage />
      </MemoryRouter>,
    );
    expect(
      screen.getByText("The purchase order could not be loaded."),
    ).toBeInTheDocument();

    usePurchaseOrdersMock.mockReturnValue({
      data: {
        items: [
          {
            id: "a",
            supplierId: "supplier",
            supplierCode: "ACME",
            supplierName: "Acme Supplies",
            status: 0,
            lines: [],
            createdAtUtc: "2026-07-25T10:00:00Z",
            updatedAtUtc: "2026-07-25T10:00:00Z",
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
        <PurchaseOrderListPage />
      </MemoryRouter>,
    );
    expect(screen.getByText("ACME — Acme Supplies")).toBeInTheDocument();
    expect(screen.getByText("Draft")).toBeInTheDocument();
    expect(
      screen.getByRole("columnheader", { name: "Order number" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("columnheader", { name: "Destination warehouse" }),
    ).toBeInTheDocument();
  });
});

function renderPage() {
  return render(
    <MemoryRouter>
      <PurchaseOrderListPage />
    </MemoryRouter>,
  );
}
