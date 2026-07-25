import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";
import { ApiError } from "../../../shared/api/apiClient";
import { SupplierListPage } from "./SupplierListPage";

const { useSuppliersMock } = vi.hoisted(() => ({ useSuppliersMock: vi.fn() }));

vi.mock("../api/useSuppliers", () => ({
  useSuppliers: useSuppliersMock,
}));

describe("SupplierListPage", () => {
  beforeEach(() => useSuppliersMock.mockReset());

  it("renders loading, empty, error, and populated states", () => {
    useSuppliersMock.mockReturnValue({
      data: undefined,
      error: null,
      isFetching: true,
      isLoading: true,
    });
    const { rerender } = renderPage();
    expect(screen.getByText("Loading suppliers…")).toBeInTheDocument();

    useSuppliersMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
      error: null,
      isFetching: false,
      isLoading: false,
    });
    rerender(<MemoryRouter><SupplierListPage /></MemoryRouter>);
    expect(screen.getByText("No suppliers exist yet.")).toBeInTheDocument();

    useSuppliersMock.mockReturnValue({
      data: undefined,
      error: new ApiError(500, {}),
      isFetching: false,
      isLoading: false,
    });
    rerender(<MemoryRouter><SupplierListPage /></MemoryRouter>);
    expect(screen.getByText("Suppliers could not be loaded.")).toBeInTheDocument();

    useSuppliersMock.mockReturnValue({
      data: {
        items: [{
          id: "a",
          code: "ACME-01",
          name: "Acme Supplies",
          email: null,
          phoneNumber: null,
          address: null,
          isActive: true,
          createdAtUtc: "2026-07-25T10:00:00Z",
          updatedAtUtc: "2026-07-25T10:00:00Z",
        }],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      },
      error: null,
      isFetching: false,
      isLoading: false,
    });
    rerender(<MemoryRouter><SupplierListPage /></MemoryRouter>);
    expect(screen.getByText("ACME-01")).toBeInTheDocument();
  });
});

function renderPage() {
  return render(<MemoryRouter><SupplierListPage /></MemoryRouter>);
}
