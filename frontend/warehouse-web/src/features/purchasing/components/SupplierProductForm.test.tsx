import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { SupplierProductForm } from "./SupplierProductForm";

const { useSuppliersMock, useProductsMock, usePurchasingCurrenciesMock } = vi.hoisted(() => ({
  useSuppliersMock: vi.fn(),
  useProductsMock: vi.fn(),
  usePurchasingCurrenciesMock: vi.fn(),
}));

vi.mock("../../suppliers/api/useSuppliers", () => ({ useSuppliers: useSuppliersMock }));
vi.mock("../../products/api/useProducts", () => ({ useProducts: useProductsMock }));
vi.mock("../api/usePurchasing", () => ({ usePurchasingCurrencies: usePurchasingCurrenciesMock }));
vi.mock("../../../shared/feedback/ApiFeedbackProvider", () => ({ useApiFeedback: () => ({ notifyError: vi.fn() }) }));

describe("SupplierProductForm", () => {
  beforeEach(() => {
    useSuppliersMock.mockReturnValue({ data: { items: [{ id: "supplier-1", code: "SUP", name: "Supplier", isActive: true }] }, isLoading: false });
    useProductsMock.mockReturnValue({
      data: {
        items: [{
          id: "product-1",
          sku: "TEST-1",
          name: "Test product",
          isActive: true,
          baseUnitOfMeasure: "EA",
          unitConversions: [{ unitOfMeasure: "CTN", quantityInBaseUnit: 24, allowsFractionalQuantity: false }],
        }],
      },
      isLoading: false,
    });
    usePurchasingCurrenciesMock.mockReturnValue({ data: [{ code: "DZD", isDefault: true }, { code: "EUR", isDefault: false }], isLoading: false });
  });

  it("uses product-defined units and defaults currency from the central catalogue", async () => {
    const user = userEvent.setup();
    render(<SupplierProductForm isSubmitting={false} submitLabel="Create" errorMessageKey="purchasing.catalogue.errors.create" onSubmit={async () => undefined} />);

    await waitFor(() => expect(screen.getByText("DZD")).toBeInTheDocument());
    expect(screen.getByRole("combobox", { name: "Purchase unit" })).toBeDisabled();

    await user.click(screen.getByRole("combobox", { name: "Product" }));
    await user.click(screen.getByText("TEST-1 — Test product"));
    await user.click(screen.getByRole("combobox", { name: "Purchase unit" }));

    expect(screen.getByText("EA (base unit)")).toBeInTheDocument();
    expect(screen.getByText("CTN — 24 EA")).toBeInTheDocument();
  });
});
