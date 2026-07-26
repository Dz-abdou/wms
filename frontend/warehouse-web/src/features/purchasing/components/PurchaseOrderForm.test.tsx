import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { PurchaseOrderForm } from "./PurchaseOrderForm";

const { useSuppliersMock, useSupplierProductsMock, usePurchasingCurrenciesMock, useWarehousesMock } = vi.hoisted(() => ({
  useSuppliersMock: vi.fn(),
  useSupplierProductsMock: vi.fn(),
  usePurchasingCurrenciesMock: vi.fn(),
  useWarehousesMock: vi.fn(),
}));

vi.mock("../../suppliers/api/useSuppliers", () => ({
  useSuppliers: useSuppliersMock,
}));
vi.mock("../../warehouses/api/useWarehouses", () => ({
  useWarehouses: useWarehousesMock,
}));
vi.mock("../api/usePurchasing", () => ({
  useSupplierProducts: useSupplierProductsMock,
  usePurchasingCurrencies: usePurchasingCurrenciesMock,
}));
vi.mock("../../../shared/feedback/ApiFeedbackProvider", () => ({
  useApiFeedback: () => ({ notifyError: vi.fn() }),
}));

describe("PurchaseOrderForm", () => {
  beforeEach(async () => {
    await i18n.changeLanguage("en");
    useSuppliersMock.mockReturnValue({
      data: {
        items: [
          { id: "supplier-1", code: "SUP", name: "Supplier", isActive: true },
        ],
      },
    });
    useSupplierProductsMock.mockReturnValue({
      data: {
        items: [
          {
            id: "catalogue-1",
            supplierId: "supplier-1",
            supplierCode: "SUP",
            supplierName: "Supplier",
            productId: "product-1",
            productSku: "SKU-1",
            productName: "Product",
            supplierSku: "SUP-SKU-1",
            purchaseUnitOfMeasure: "CTN",
            minimumOrderQuantity: 2,
            unitPrice: 10,
            currencyCode: "DZD",
            isActive: true,
            createdAtUtc: "2026-07-25T00:00:00Z",
            updatedAtUtc: "2026-07-25T00:00:00Z",
          },
        ],
      },
      isLoading: false,
    });
    useWarehousesMock.mockReturnValue({ data: { items: [] } });
    usePurchasingCurrenciesMock.mockReturnValue({ data: [] });
  });

  it("uses an editable table for purchase-order lines", async () => {
    const user = userEvent.setup();
    render(
      <PurchaseOrderForm
        errorMessageKey="purchasing.orders.errors.create"
        isSubmitting={false}
        onSubmit={async () => undefined}
        submitLabel="Create draft"
      />,
    );

    expect(screen.getByRole("button", { name: "Add line" })).toBeDisabled();
    await user.click(screen.getByRole("combobox", { name: "Supplier" }));
    await user.click(await screen.findByText("SUP — Supplier"));
    await user.click(screen.getByRole("button", { name: "Add line" }));

    expect(
      screen.getByRole("columnheader", { name: "Supplier SKU" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("columnheader", { name: "Line total" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("combobox", { name: "Supplier catalogue item" }),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole("combobox", { name: "Supplier catalogue item" }),
    );
    await user.click(await screen.findByText("SKU-1 — Product"));
  });

  it("shows the MOQ error beside an existing invalid quantity", async () => {
    const user = userEvent.setup();
    render(
      <PurchaseOrderForm
        errorMessageKey="purchasing.orders.errors.create"
        initialValues={{
          supplierId: "supplier-1",
          lines: [{ supplierProductId: "catalogue-1", quantity: 1 }],
        }}
        isSubmitting={false}
        onSubmit={async () => undefined}
        submitLabel="Create draft"
      />,
    );

    await user.click(screen.getByRole("button", { name: "Create draft" }));

    expect(
      await screen.findByText("Quantity must be at least 2."),
    ).toBeInTheDocument();
  });
});
