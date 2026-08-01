import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/apiClient";
import { i18n } from "../../../shared/i18n/i18n";
import { CustomerForm } from "../components/CustomerForm";

const { usePurchasingCurrenciesMock } = vi.hoisted(() => ({
  usePurchasingCurrenciesMock: vi.fn(),
}));

vi.mock("../../purchasing/api/usePurchasing", () => ({
  usePurchasingCurrencies: usePurchasingCurrenciesMock,
}));

describe("CustomerForm", () => {
  beforeEach(() => {
    usePurchasingCurrenciesMock.mockReturnValue({
      data: [{ code: "DZD", isDefault: true }],
      isLoading: false,
    });
  });

  afterEach(async () => {
    await i18n.changeLanguage("en");
  });

  it("blocks blank code and legal name before submitting", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    renderForm(onSubmit);

    await user.click(screen.getByRole("button", { name: "Create customer" }));

    expect(
      await screen.findByText("Customer code is required."),
    ).toBeInTheDocument();
    expect(
      await screen.findByText("Customer legal name is required."),
    ).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("translates a server validation error on the default-currency field", async () => {
    await i18n.changeLanguage("fr");
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(
      new ApiError(422, {
        errorCodes: {
          DefaultCurrencyCode: ["customer.default_currency_not_supported"],
        },
        errors: { DefaultCurrencyCode: ["Currency is inactive."] },
      }),
    );

    render(
      <CustomerForm
        cancelLabel="Annuler"
        errorMessageKey="customers.errors.create"
        isSubmitting={false}
        onCancel={() => undefined}
        onSubmit={onSubmit}
        submitLabel="Créer le client"
      />,
    );
    await user.type(screen.getByLabelText("Code"), "CUSTOMER-001");
    await user.type(screen.getByLabelText("Raison sociale"), "Acme");
    await user.click(screen.getByRole("button", { name: "Créer le client" }));

    await waitFor(() =>
      expect(
        screen.getByText("La devise par défaut sélectionnée n’est pas active."),
      ).toBeInTheDocument(),
    );
  });
});

function renderForm(onSubmit: () => Promise<void>) {
  return render(
    <CustomerForm
      cancelLabel="Cancel"
      errorMessageKey="customers.errors.create"
      isSubmitting={false}
      onCancel={() => undefined}
      onSubmit={onSubmit}
      submitLabel="Create customer"
    />,
  );
}
