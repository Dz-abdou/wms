import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/apiClient";
import { i18n } from "../../../shared/i18n/i18n";
import { ProductForm } from "./ProductForm";

vi.mock("../api/useProductCategories", () => ({
  useProductCategories: () => ({ data: { items: [] }, isLoading: false }),
}));

describe("ProductForm", () => {
  beforeEach(async () => {
    await i18n.changeLanguage("en");
  });

  it("uses an editable table for packaging conversions", async () => {
    const user = userEvent.setup();

    render(
      <ProductForm
        isSubmitting={false}
        onSubmit={vi.fn()}
        submitLabel="Create product"
      />,
    );
    await user.click(screen.getByRole("button", { name: "Add conversion" }));

    expect(
      screen.getByRole("columnheader", { name: "Unit (for example, CTN)" }),
    ).toBeInTheDocument();
    expect(
      screen.getByPlaceholderText("Unit (for example, CTN)"),
    ).toBeInTheDocument();
  });

  it("blocks a blank SKU and name before submitting", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();

    render(
      <ProductForm
        isSubmitting={false}
        onSubmit={onSubmit}
        submitLabel="Create product"
      />,
    );
    await user.click(screen.getByRole("button", { name: "Create product" }));

    expect(await screen.findByText("SKU is required.")).toBeInTheDocument();
    expect(await screen.findByText("Name is required.")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("translates server validation error codes on their fields", async () => {
    await i18n.changeLanguage("fr");
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(
      new ApiError(400, {
        errorCodes: { Sku: ["validation.required"] },
        errors: { Sku: ["SKU is required."] },
      }),
    );

    render(
      <ProductForm
        isSubmitting={false}
        onSubmit={onSubmit}
        submitLabel="Créer le produit"
      />,
    );
    await user.type(screen.getByLabelText("Référence"), "SKU-001");
    await user.type(screen.getByLabelText("Nom"), "Sample product");
    await user.click(screen.getByRole("button", { name: "Créer le produit" }));

    await waitFor(() =>
      expect(screen.getByText("Ce champ est obligatoire.")).toBeInTheDocument(),
    );
  });

  it("shows a nested measurement error beside the responsible field", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(
      new ApiError(400, {
        errorCodes: {
          "Measurements.WeightUnitOfMeasure": [
            "product.measurement_weight_unit_required",
          ],
        },
        errors: {
          "Measurements.WeightUnitOfMeasure": [
            "A weight unit is required when a product weight is supplied.",
          ],
        },
      }),
    );

    render(
      <ProductForm
        isSubmitting={false}
        onSubmit={onSubmit}
        submitLabel="Create product"
      />,
    );
    await user.type(screen.getByLabelText("SKU"), "WEIGHT-001");
    await user.type(screen.getByLabelText("Name"), "Weighted product");
    await user.click(screen.getByRole("button", { name: "Create product" }));

    await waitFor(() =>
      expect(
        screen.getByText("Select a weight unit when entering a weight."),
      ).toBeInTheDocument(),
    );
  });
});
