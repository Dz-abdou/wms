import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";
import { ApiError } from "../../../shared/api/apiClient";
import { CustomerListPage } from "../pages/CustomerListPage";

const { useCustomersMock } = vi.hoisted(() => ({ useCustomersMock: vi.fn() }));

vi.mock("../api/useCustomers", () => ({
  useCustomers: useCustomersMock,
}));

describe("CustomerListPage", () => {
  beforeEach(() => useCustomersMock.mockReset());

  it("renders loading, empty, error, and populated states", () => {
    useCustomersMock.mockReturnValue({
      data: undefined,
      error: null,
      isFetching: true,
      isLoading: true,
    });
    const { rerender } = renderPage();
    expect(screen.getByText("Loading customers…")).toBeInTheDocument();

    useCustomersMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
      error: null,
      isFetching: false,
      isLoading: false,
    });
    rerender(<Page />);
    expect(
      screen.getByText("No customers match this search."),
    ).toBeInTheDocument();

    useCustomersMock.mockReturnValue({
      data: undefined,
      error: new ApiError(500, {}),
      isFetching: false,
      isLoading: false,
    });
    rerender(<Page />);
    expect(
      screen.getByText("Customers could not be loaded."),
    ).toBeInTheDocument();

    useCustomersMock.mockReturnValue({
      data: {
        items: [
          {
            id: "a",
            code: "ACME-01",
            legalName: "Acme Distribution",
            tradingName: "Acme",
            defaultCurrencyCode: "DZD",
            isActive: true,
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
    rerender(<Page />);
    expect(screen.getByText("ACME-01")).toBeInTheDocument();
  });
});

function Page() {
  return (
    <MemoryRouter>
      <CustomerListPage />
    </MemoryRouter>
  );
}

function renderPage() {
  return render(<Page />);
}
