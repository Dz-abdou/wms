import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/apiClient";
import { i18n } from "../../../shared/i18n/i18n";
import { PurchaseOrderForm } from "./PurchaseOrderForm";

const { useSuppliersMock, useSupplierProductsMock, useWarehousesMock } =
  vi.hoisted(() => ({
    useSuppliersMock: vi.fn(),
    useSupplierProductsMock: vi.fn(),
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
          {
            id: "supplier-1",
            code: "SUP",
            name: "Supplier",
            defaultCurrencyCode: "DZD",
            isActive: true,
          },
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

    expect(screen.getByText("DZD")).toBeInTheDocument();
    const currency = screen.getByRole("combobox", { name: "Currency" });
    expect(currency.closest(".ant-select")).toHaveClass("ant-select-disabled");
  }, 20_000);

  it("filters catalogue items and locks a pre-line currency override after a line is selected", async () => {
    const user = userEvent.setup();
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
            productName: "Dinar product",
            supplierSku: "SUP-DZD",
            purchaseUnitOfMeasure: "CTN",
            minimumOrderQuantity: 2,
            unitPrice: 10,
            currencyCode: "DZD",
            isActive: true,
            createdAtUtc: "2026-07-25T00:00:00Z",
            updatedAtUtc: "2026-07-25T00:00:00Z",
          },
          {
            id: "catalogue-2",
            supplierId: "supplier-1",
            supplierCode: "SUP",
            supplierName: "Supplier",
            productId: "product-2",
            productSku: "SKU-2",
            productName: "Dollar product",
            supplierSku: "SUP-USD",
            purchaseUnitOfMeasure: "CTN",
            minimumOrderQuantity: 2,
            unitPrice: 10,
            currencyCode: "USD",
            isActive: true,
            createdAtUtc: "2026-07-25T00:00:00Z",
            updatedAtUtc: "2026-07-25T00:00:00Z",
          },
        ],
      },
      isLoading: false,
    });
    render(
      <PurchaseOrderForm
        errorMessageKey="purchasing.orders.errors.create"
        initialValues={{
          supplierId: "supplier-1",
          currencyCode: "USD",
          lines: [{ supplierProductId: "catalogue-2", quantity: 2 }],
        }}
        isSubmitting={false}
        onSubmit={async () => undefined}
        submitLabel="Create draft"
      />,
    );

    await user.click(screen.getByRole("button", { name: "Add line" }));

    const catalogueSelectors = screen.getAllByRole("combobox", {
      name: "Supplier catalogue item",
    });
    await user.click(catalogueSelectors[1]);

    const options = screen.getByRole("listbox");
    expect(
      within(options).getByRole("option", {
        name: "SKU-2 — Dollar product",
      }),
    ).toBeInTheDocument();
    expect(
      within(options).queryByRole("option", {
        name: "SKU-1 — Dinar product",
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole("combobox", { name: "Currency" }).closest(".ant-select"),
    ).toHaveClass("ant-select-disabled");
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

  it("shows a server quantity-unit error beside the line quantity", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(
      new ApiError(422, {
        errorCodes: {
          "Lines[0].Quantity": ["purchase_order.quantity_unit_invalid"],
        },
        errors: {
          "Lines[0].Quantity": [
            "The quantity is not valid for the selected purchase unit.",
          ],
        },
      }),
    );

    render(
      <PurchaseOrderForm
        errorMessageKey="purchasing.orders.errors.create"
        initialValues={{
          supplierId: "supplier-1",
          destinationWarehouseId: "warehouse-1",
          currencyCode: "DZD",
          orderDate: "2026-07-27",
          lines: [{ supplierProductId: "catalogue-1", quantity: 100.3 }],
        }}
        isSubmitting={false}
        onSubmit={onSubmit}
        submitLabel="Create draft"
      />,
    );

    await user.click(screen.getByRole("button", { name: "Create draft" }));

    await waitFor(() =>
      expect(
        screen.getByText(
          "Quantity must be a whole number for the selected unit.",
        ),
      ).toBeInTheDocument(),
    );
  });
});
