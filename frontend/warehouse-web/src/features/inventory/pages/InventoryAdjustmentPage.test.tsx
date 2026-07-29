import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { InventoryAdjustmentPage } from "./InventoryAdjustmentPage";

const { useProductsMock, useWarehousesMock, useAdjustInventoryMock } =
  vi.hoisted(() => ({
    useProductsMock: vi.fn(),
    useWarehousesMock: vi.fn(),
    useAdjustInventoryMock: vi.fn(),
  }));

vi.mock("../../products/api/useProducts", () => ({
  useProducts: useProductsMock,
}));
vi.mock("../../warehouses/api/useWarehouses", () => ({
  useWarehouses: useWarehousesMock,
}));
vi.mock("../api/useInventory", () => ({
  useAdjustInventory: useAdjustInventoryMock,
}));

describe("InventoryAdjustmentPage", () => {
  beforeEach(async () => {
    await i18n.changeLanguage("en");
    useProductsMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 100, totalCount: 0 },
      error: null,
      isLoading: false,
    });
    useWarehousesMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 100, totalCount: 0 },
      error: null,
      isLoading: false,
    });
    useAdjustInventoryMock.mockReturnValue({
      data: undefined,
      error: null,
      isPending: false,
      mutateAsync: vi.fn(),
    });
  });

  it("renders only the adjustment workflow", () => {
    renderPage();

    expect(
      screen.getByRole("heading", { name: "Manual adjustment" }),
    ).toBeInTheDocument();
    expect(screen.queryByText("Movement history")).not.toBeInTheDocument();
  });

  it("renders a source loading state", () => {
    useProductsMock.mockReturnValue({
      data: undefined,
      error: null,
      isLoading: true,
    });

    renderPage();

    expect(
      screen.getByText("Loading products and warehouses…"),
    ).toBeInTheDocument();
  });

  it("selects the product base and conversion units for an adjustment", async () => {
    const user = userEvent.setup();
    useProductsMock.mockReturnValue({
      data: {
        items: [
          {
            id: "product-1",
            sku: "CTN-001",
            name: "Cartons",
            description: null,
            baseUnitOfMeasure: "EA",
            unitConversions: [
              {
                unitOfMeasure: "CTN",
                quantityInBaseUnit: 24,
                allowsFractionalQuantity: false,
              },
            ],
            measurements: null,
            categoryId: null,
            isActive: true,
            createdAtUtc: "2026-07-23T00:00:00Z",
            updatedAtUtc: "2026-07-23T00:00:00Z",
          },
        ],
        page: 1,
        pageSize: 100,
        totalCount: 1,
      },
      error: null,
      isLoading: false,
    });

    renderPage();
    await user.click(screen.getByRole("button", { name: "Add line" }));
    await user.click(screen.getByLabelText("Product"));
    await user.click(await screen.findByText("CTN-001 — Cartons"));

    expect(await screen.findByText("EA")).toBeInTheDocument();
    await user.click(screen.getByLabelText("Unit of measure"));
    expect(screen.getAllByText("CTN")).not.toHaveLength(0);
  }, 20_000);
});

function renderPage() {
  return render(
    <MemoryRouter>
      <InventoryAdjustmentPage />
    </MemoryRouter>,
  );
}
