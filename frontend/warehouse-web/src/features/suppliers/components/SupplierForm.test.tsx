import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/apiClient";
import { i18n } from "../../../shared/i18n/i18n";
import { SupplierForm } from "./SupplierForm";

const { usePurchasingCurrenciesMock } = vi.hoisted(() => ({
  usePurchasingCurrenciesMock: vi.fn(),
}));

vi.mock("../../purchasing/api/usePurchasing", () => ({
  usePurchasingCurrencies: usePurchasingCurrenciesMock,
}));

describe("SupplierForm", () => {
  beforeEach(() => {
    usePurchasingCurrenciesMock.mockReturnValue({
      data: [{ code: "DZD", isDefault: true }],
      isLoading: false,
    });
  });

  afterEach(async () => {
    await i18n.changeLanguage("en");
  });

  it("blocks blank code and name before submitting", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();

    render(
      <SupplierForm
        errorMessageKey="suppliers.errors.create"
        isSubmitting={false}
        onSubmit={onSubmit}
        submitLabel="Create supplier"
      />,
    );
    await user.click(screen.getByRole("button", { name: "Create supplier" }));

    expect(
      await screen.findByText("Supplier code is required."),
    ).toBeInTheDocument();
    expect(
      await screen.findByText("Supplier name is required."),
    ).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("translates server validation error codes on their fields", async () => {
    await i18n.changeLanguage("fr");
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(
      new ApiError(400, {
        errorCodes: { Code: ["validation.required"] },
        errors: { Code: ["Supplier code is required."] },
      }),
    );

    render(
      <SupplierForm
        errorMessageKey="suppliers.errors.create"
        isSubmitting={false}
        onSubmit={onSubmit}
        submitLabel="Créer le fournisseur"
      />,
    );
    await user.type(screen.getByLabelText("Code"), "SUPPLIER-001");
    await user.type(screen.getByLabelText("Nom"), "Acme Supplies");
    await user.click(
      screen.getByRole("button", { name: "Créer le fournisseur" }),
    );

    await waitFor(() =>
      expect(screen.getByText("Ce champ est obligatoire.")).toBeInTheDocument(),
    );
  });

  it("defaults the supplier currency from the central currency catalogue", async () => {
    render(
      <SupplierForm
        errorMessageKey="suppliers.errors.create"
        isSubmitting={false}
        onSubmit={async () => undefined}
        submitLabel="Create supplier"
      />,
    );

    expect(screen.getByText("DZD")).toBeInTheDocument();
  });
});
