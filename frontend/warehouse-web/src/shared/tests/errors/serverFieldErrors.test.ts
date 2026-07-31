import type { FormInstance } from "antd";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../api/apiClient";
import { i18n } from "../../i18n/i18n";
import { applyServerFieldErrors } from "../../errors/serverFieldErrors";

describe("applyServerFieldErrors", () => {
  beforeEach(async () => {
    await i18n.changeLanguage("en");
  });

  it("maps an insufficient-stock response to the exact quantity cell", () => {
    const setFields = vi.fn();
    const form = { setFields } as unknown as FormInstance;
    const error = new ApiError(422, {
      code: "inventory.insufficient_stock",
      errors: { "Lines[0].Quantity": ["diagnostic only"] },
      errorCodes: {
        "Lines[0].Quantity": ["inventory.insufficient_stock"],
      },
      errorParameters: {
        "Lines[0].Quantity": [
          {
            availableQuantity: 3,
            baseUnitOfMeasure: "EA",
            warehouse: "MAIN — Main warehouse",
          },
        ],
      },
    });

    const handled = applyServerFieldErrors(
      form,
      error,
      i18n.t.bind(i18n),
      "errors.validationFailed",
    );

    expect(handled).toBe(true);
    expect(setFields).toHaveBeenCalledWith([
      {
        name: ["lines", 0, "quantity"],
        errors: [
          "The quantity exceeds available stock in MAIN — Main warehouse. Available: 3 EA.",
        ],
      },
    ]);
  });
});
