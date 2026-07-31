import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { InventoryTransferPage } from "../pages/InventoryTransferPage";

const {
  getTransferCandidateMock,
  useCreateTransferMock,
  useProductsMock,
  useWarehousesMock,
} = vi.hoisted(() => ({
  getTransferCandidateMock: vi.fn(),
  useCreateTransferMock: vi.fn(),
  useProductsMock: vi.fn(),
  useWarehousesMock: vi.fn(),
}));

vi.mock("../../products/api/useProducts", () => ({
  useProducts: useProductsMock,
}));
vi.mock("../../warehouses/api/useWarehouses", () => ({
  useWarehouses: useWarehousesMock,
}));
vi.mock("../api/inventoryApi", () => ({
  getTransferCandidate: getTransferCandidateMock,
}));
vi.mock("../api/useInventory", () => ({
  useCreateTransfer: useCreateTransferMock,
}));

describe("InventoryTransferPage", () => {
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
    useCreateTransferMock.mockReturnValue({
      isPending: false,
      mutateAsync: vi.fn(),
    });
  });

  it("requires both warehouses before transfer lines can be added", () => {
    render(
      <MemoryRouter>
        <InventoryTransferPage />
      </MemoryRouter>,
    );

    expect(
      screen.getByRole("heading", { name: "Record transfer" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Add line" })).toBeDisabled();
  });
});
