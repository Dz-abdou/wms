import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/apiClient";
import { SupplierProductForm } from "./SupplierProductForm";

const { useSuppliersMock, useProductsMock, usePurchasingCurrenciesMock } =
  vi.hoisted(() => ({
    useSuppliersMock: vi.fn(),
    useProductsMock: vi.fn(),
    usePurchasingCurrenciesMock: vi.fn(),
  }));

vi.mock("../../suppliers/api/useSuppliers", () => ({
  useSuppliers: useSuppliersMock,
}));
vi.mock("../../products/api/useProducts", () => ({
  useProducts: useProductsMock,
}));
vi.mock("../api/usePurchasing", () => ({
  usePurchasingCurrencies: usePurchasingCurrenciesMock,
}));
vi.mock("../../../shared/feedback/ApiFeedbackProvider", () => ({
  useApiFeedback: () => ({ notifyError: vi.fn() }),
}));

describe("SupplierProductForm", () => {
  beforeEach(() => {
    useSuppliersMock.mockReturnValue({
      data: {
        items: [
          { id: "supplier-1", code: "SUP", name: "Supplier", isActive: true },
        ],
      },
      isLoading: false,
    });
    useProductsMock.mockReturnValue({
      data: {
        items: [
          {
            id: "product-1",
            sku: "TEST-1",
            name: "Test product",
            isActive: true,
            baseUnitOfMeasure: "EA",
            unitConversions: [
              {
                unitOfMeasure: "CTN",
                quantityInBaseUnit: 24,
                allowsFractionalQuantity: false,
              },
            ],
          },
        ],
      },
      isLoading: false,
    });
    usePurchasingCurrenciesMock.mockReturnValue({
      data: [
        { code: "DZD", isDefault: true },
        { code: "EUR", isDefault: false },
      ],
      isLoading: false,
    });
  });

  it("defaults currency from the central catalogue and requires a product before units can be selected", async () => {
    render(
      <SupplierProductForm
        isSubmitting={false}
        submitLabel="Create"
        errorMessageKey="purchasing.catalogue.errors.create"
        onSubmit={async () => undefined}
      />,
    );

    await waitFor(() => expect(screen.getByText("DZD")).toBeInTheDocument());
    expect(
      screen.getByRole("combobox", { name: "Purchase unit" }),
    ).toBeDisabled();
  });

  it("shows the translated server error beside the affected catalogue field", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(
      new ApiError(422, {
        errorCodes: {
          CurrencyCode: ["supplier_product.currency_not_supported"],
        },
        errors: {
          CurrencyCode: ["Currency 'ZZZ' is not enabled for purchasing."],
        },
      }),
    );

    render(
      <SupplierProductForm
        errorMessageKey="purchasing.catalogue.errors.create"
        initialValues={{
          supplierId: "supplier-1",
          productId: "product-1",
          purchaseUnitOfMeasure: "EA",
          minimumOrderQuantity: 1,
          unitPrice: 20,
          currencyCode: "DZD",
        }}
        isSubmitting={false}
        onSubmit={onSubmit}
        submitLabel="Create"
      />,
    );

    await user.click(screen.getByRole("button", { name: "Create" }));

    await waitFor(() =>
      expect(
        screen.getByText(
          "Select an active currency from the currency catalogue.",
        ),
      ).toBeInTheDocument(),
    );
  });
});
