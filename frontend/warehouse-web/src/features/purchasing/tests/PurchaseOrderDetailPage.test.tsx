import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { PurchaseOrderDetailPage } from "../pages/PurchaseOrderDetailPage";

const {
  useCancelPurchaseOrderMock,
  usePurchaseOrderMock,
  useSubmitPurchaseOrderMock,
} = vi.hoisted(() => ({
  useCancelPurchaseOrderMock: vi.fn(),
  usePurchaseOrderMock: vi.fn(),
  useSubmitPurchaseOrderMock: vi.fn(),
}));

vi.mock("../api/usePurchasing", () => ({
  useCancelPurchaseOrder: useCancelPurchaseOrderMock,
  usePurchaseOrder: usePurchaseOrderMock,
  useSubmitPurchaseOrder: useSubmitPurchaseOrderMock,
}));

describe("PurchaseOrderDetailPage", () => {
  beforeEach(async () => {
    await i18n.changeLanguage("en");
    useCancelPurchaseOrderMock.mockReturnValue({ isPending: false });
    useSubmitPurchaseOrderMock.mockReturnValue({ isPending: false });
  });

  it("shows draft actions for the named API status", () => {
    usePurchaseOrderMock.mockReturnValue({
      data: createOrder("Draft"),
      error: null,
      isLoading: false,
    });

    renderPage();

    expect(screen.getByRole("button", { name: "Edit" })).toBeVisible();
    expect(screen.getByRole("button", { name: "Submit order" })).toBeVisible();
    expect(screen.getByRole("button", { name: "Cancel order" })).toBeVisible();
  });

  it("shows receive for the named submitted status", () => {
    usePurchaseOrderMock.mockReturnValue({
      data: createOrder("Submitted"),
      error: null,
      isLoading: false,
    });

    renderPage();

    expect(
      screen.getByRole("button", { name: "Receive goods" }),
    ).toBeVisible();
  });
});

function createOrder(status: "Draft" | "Submitted") {
  return {
    buyerUserId: "buyer-1",
    createdAtUtc: "2026-08-01T10:00:00Z",
    currencyCode: "DZD",
    destinationWarehouseCode: "MAIN",
    destinationWarehouseId: "warehouse-1",
    destinationWarehouseName: "Main warehouse",
    id: "order-1",
    lines: [],
    number: "PO-2026-000001",
    orderDate: "2026-08-01",
    status,
    statusHistory: [],
    supplierCode: "SUP",
    supplierId: "supplier-1",
    supplierName: "Supplier",
    totalAmount: 0,
    updatedAtUtc: "2026-08-01T10:00:00Z",
    version: 1,
  };
}

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/purchase-orders/order-1"]}>
      <Routes>
        <Route
          element={<PurchaseOrderDetailPage />}
          path="/purchase-orders/:id"
        />
      </Routes>
    </MemoryRouter>,
  );
}
