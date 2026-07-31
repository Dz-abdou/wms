import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { InventoryOverviewPage } from "../pages/InventoryOverviewPage";

const {
  useProductCategoriesMock,
  useWarehousesMock,
  useInventoryOverviewMock,
} = vi.hoisted(() => ({
  useProductCategoriesMock: vi.fn(),
  useWarehousesMock: vi.fn(),
  useInventoryOverviewMock: vi.fn(),
}));

vi.mock("../../products/api/useProductCategories", () => ({
  useProductCategories: useProductCategoriesMock,
}));
vi.mock("../../warehouses/api/useWarehouses", () => ({
  useWarehouses: useWarehousesMock,
}));
vi.mock("../api/useInventory", () => ({
  useInventoryOverview: useInventoryOverviewMock,
}));

describe("InventoryOverviewPage", () => {
  beforeEach(async () => {
    await i18n.changeLanguage("en");
    useProductCategoriesMock.mockReturnValue({
      data: { items: [] },
      error: null,
      isLoading: false,
    });
    useWarehousesMock.mockReturnValue({
      data: { items: [] },
      error: null,
      isLoading: false,
    });
  });

  it("renders a localized empty state", () => {
    useInventoryOverviewMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
      error: null,
      isFetching: false,
      isLoading: false,
    });

    renderPage();

    expect(
      screen.getByText("No inventory balances match these filters."),
    ).toBeInTheDocument();
  });

  it("shows on-hand stock and links to the filtered movement ledger", () => {
    useInventoryOverviewMock.mockReturnValue({
      data: {
        items: [
          {
            productId: "product-1",
            productSku: "SKU-001",
            productName: "Sample product",
            productIsActive: true,
            warehouseId: "warehouse-1",
            warehouseCode: "MAIN",
            warehouseName: "Main warehouse",
            quantity: 12,
            baseUnitOfMeasure: "EA",
            updatedAtUtc: "2026-07-29T12:00:00Z",
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

    renderPage();

    expect(screen.getByText("SKU-001 — Sample product")).toBeInTheDocument();
    expect(screen.getByText("12")).toBeInTheDocument();
    expect(screen.getByText("EA")).toBeInTheDocument();
    expect(screen.getByText("Active")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "View history" })).toHaveAttribute(
      "href",
      "/inventory/movements?productId=product-1&warehouseId=warehouse-1",
    );
  });
});

function renderPage() {
  return render(
    <MemoryRouter>
      <InventoryOverviewPage />
    </MemoryRouter>,
  );
}
