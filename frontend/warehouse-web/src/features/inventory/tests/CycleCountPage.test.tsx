import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { CycleCountPage } from "../pages/CycleCountPage";

const {
  getCycleCountCandidateMock,
  useProductsMock,
  useWarehousesMock,
  useCreateCycleCountMock,
} = vi.hoisted(() => ({
  getCycleCountCandidateMock: vi.fn(),
  useProductsMock: vi.fn(),
  useWarehousesMock: vi.fn(),
  useCreateCycleCountMock: vi.fn(),
}));

vi.mock("../../products/api/useProducts", () => ({
  useProducts: useProductsMock,
}));
vi.mock("../../warehouses/api/useWarehouses", () => ({
  useWarehouses: useWarehousesMock,
}));
vi.mock("../api/useInventory", () => ({
  useCreateCycleCount: useCreateCycleCountMock,
}));
vi.mock("../api/inventoryApi", () => ({
  getCycleCountCandidate: getCycleCountCandidateMock,
}));

describe("CycleCountPage", () => {
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
    useCreateCycleCountMock.mockReturnValue({
      isPending: false,
      mutateAsync: vi.fn(),
    });
  });

  it("requires a warehouse before count lines can be added", () => {
    render(
      <MemoryRouter>
        <CycleCountPage />
      </MemoryRouter>,
    );

    expect(
      screen.getByRole("heading", { name: "Record cycle count" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Add line" })).toBeDisabled();
  });

  it("submits the loaded balance version for each count line", async () => {
    const user = userEvent.setup();
    const mutateAsync = vi.fn().mockResolvedValue({ id: "count-1" });
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
            name: "Main warehouse",
          },
        ],
        page: 1,
        pageSize: 100,
        totalCount: 1,
      },
      error: null,
      isLoading: false,
    });
    useCreateCycleCountMock.mockReturnValue({
      isPending: false,
      mutateAsync,
    });
    getCycleCountCandidateMock.mockResolvedValue({
      baseUnitOfMeasure: "EA",
      productId: "product-1",
      systemBalanceVersion: 4,
      systemQuantityInBase: 70,
    });

    renderPage();

    await user.click(screen.getByLabelText("Warehouse"));
    await user.click(await screen.findByText("WH-001 — Main warehouse"));
    await user.click(screen.getByRole("button", { name: "Add line" }));
    await user.click(screen.getByLabelText("Product"));
    await user.click(await screen.findByText("EA-001 — Eaches"));
    await waitFor(() => {
      expect(getCycleCountCandidateMock).toHaveBeenCalledWith(
        "warehouse-1",
        "product-1",
      );
    });
    await user.type(screen.getByLabelText("Counted quantity"), "70");
    await user.click(screen.getByRole("button", { name: "Post cycle count" }));

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({
          lines: [
            expect.objectContaining({
              productId: "product-1",
              systemBalanceVersion: 4,
              systemQuantityInBase: 70,
            }),
          ],
          warehouseId: "warehouse-1",
        }),
      );
    });
  }, 20_000);
});

function renderPage() {
  return render(
    <MemoryRouter>
      <CycleCountPage />
    </MemoryRouter>,
  );
}
