import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { InventoryMovementHistoryPage } from "./InventoryMovementHistoryPage";

const { useProductsMock, useWarehousesMock, useMovementHistoryMock } = vi.hoisted(
  () => ({
    useProductsMock: vi.fn(),
    useWarehousesMock: vi.fn(),
    useMovementHistoryMock: vi.fn(),
  }),
);

vi.mock("../../products/api/useProducts", () => ({
  useProducts: useProductsMock,
}));
vi.mock("../../warehouses/api/useWarehouses", () => ({
  useWarehouses: useWarehousesMock,
}));
vi.mock("../api/useInventory", () => ({
  useMovementHistory: useMovementHistoryMock,
}));

describe("InventoryMovementHistoryPage", () => {
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
    useMovementHistoryMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
      error: null,
      isLoading: false,
    });
  });

  it("keeps history investigation separate from the adjustment workflow", () => {
    render(
      <MemoryRouter>
        <InventoryMovementHistoryPage />
      </MemoryRouter>,
    );

    expect(
      screen.getByRole("heading", { name: "Movement history" }),
    ).toBeInTheDocument();
    expect(screen.getByText("No inventory movements exist for this selection.")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Record adjustment" }),
    ).toBeInTheDocument();
  });
});
