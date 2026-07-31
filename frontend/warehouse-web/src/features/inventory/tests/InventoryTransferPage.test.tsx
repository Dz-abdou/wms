import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
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

  it("shows the loaded source availability for a selected product", async () => {
    const user = userEvent.setup();
    useProductsMock.mockReturnValue({
      data: {
        items: [
          {
            id: "product-1",
            sku: "EA-001",
            name: "Eaches",
            baseUnitOfMeasure: "EA",
            isActive: true,
            unitConversions: [],
          },
        ],
        page: 1,
        pageSize: 100,
        totalCount: 1,
      },
      error: null,
      isLoading: false,
    });
    useWarehousesMock.mockReturnValue({
      data: {
        items: [
          {
            code: "WH-001",
            id: "warehouse-1",
            isActive: true,
            name: "Source",
          },
          {
            code: "WH-002",
            id: "warehouse-2",
            isActive: true,
            name: "Destination",
          },
        ],
        page: 1,
        pageSize: 100,
        totalCount: 2,
      },
      error: null,
      isLoading: false,
    });
    getTransferCandidateMock.mockResolvedValue({
      availableQuantityInBase: 7,
      baseUnitOfMeasure: "EA",
      productId: "product-1",
    });

    render(
      <MemoryRouter>
        <InventoryTransferPage />
      </MemoryRouter>,
    );

    await user.click(screen.getByLabelText("Source warehouse"));
    await user.click(await screen.findByText("WH-001 — Source"));
    await user.click(screen.getByLabelText("Destination warehouse"));
    const destinationOptions = await screen.findAllByText(
      "WH-002 — Destination",
    );
    await user.click(destinationOptions.at(-1)!);
    await user.click(screen.getByRole("button", { name: "Add line" }));
    await user.click(screen.getByLabelText("Product"));
    await user.click(await screen.findByText("EA-001 — Eaches"));

    expect(await screen.findByDisplayValue("7 EA")).toBeDisabled();
  }, 20_000);
});
