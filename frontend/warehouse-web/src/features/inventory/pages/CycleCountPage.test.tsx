import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { CycleCountPage } from "./CycleCountPage";

const { useProductsMock, useWarehousesMock, useCreateCycleCountMock } =
  vi.hoisted(() => ({
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
});
