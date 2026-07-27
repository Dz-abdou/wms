import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { ApiError } from "../../../shared/api/apiClient";
import { GoodsReceiptCreatePage } from "./GoodsReceiptCreatePage";

const {
  createReceiptMock,
  createReceiptState,
  notifyErrorMock,
  refetchMock,
  useCandidateMock,
} = vi.hoisted(() => ({
  createReceiptMock: vi.fn(),
  createReceiptState: {
    error: null as unknown,
    isPending: false,
    mutateAsync: vi.fn(),
  },
  notifyErrorMock: vi.fn(),
  refetchMock: vi.fn(),
  useCandidateMock: vi.fn(),
}));

vi.mock("../api/useReceiving", () => ({
  useCreateGoodsReceipt: () => createReceiptState,
  useGoodsReceiptCandidate: useCandidateMock,
}));
vi.mock("../../../shared/feedback/ApiFeedbackProvider", () => ({
  useApiFeedback: () => ({ notifyError: notifyErrorMock }),
}));

describe("GoodsReceiptCreatePage", () => {
  beforeEach(() => {
    createReceiptMock.mockReset();
    createReceiptState.error = null;
    createReceiptState.mutateAsync = createReceiptMock;
    notifyErrorMock.mockReset();
    refetchMock.mockReset();
    useCandidateMock.mockReturnValue({
      data: candidate,
      error: null,
      isLoading: false,
      refetch: refetchMock,
    });
  });

  it("defaults from the candidate and maps an over-receipt error to the quantity cell", async () => {
    createReceiptMock.mockRejectedValue(
      new ApiError(422, {
        code: "goods_receipt.over_receipt",
        errors: { "Lines[0].AcceptedQuantity": ["diagnostic"] },
        errorCodes: {
          "Lines[0].AcceptedQuantity": ["goods_receipt.over_receipt"],
        },
      }),
    );
    const user = userEvent.setup();
    renderPage();

    const quantity = screen.getByRole("spinbutton", {
      name: "Accepted now",
    });
    expect(quantity).toHaveValue("10.000000");
    await user.click(screen.getByRole("button", { name: "Post receipt" }));

    await waitFor(() =>
      expect(
        screen.getByText(
          "Accepted quantity cannot exceed the outstanding purchase-order quantity.",
        ),
      ).toBeInTheDocument(),
    );
    expect(notifyErrorMock).not.toHaveBeenCalled();
  });

  it("keeps entered quantities when refreshed after a stale receipt conflict", async () => {
    createReceiptState.error = new ApiError(409, {
      code: "goods_receipt.purchase_order_concurrency_conflict",
    });
    renderPage();

    const quantity = screen.getByRole("spinbutton", {
      name: "Accepted now",
    });
    fireEvent.change(quantity, { target: { value: "4" } });
    await userEvent.click(
      screen.getByRole("button", { name: "Refresh quantities" }),
    );

    expect(refetchMock).toHaveBeenCalledOnce();
    expect(quantity).toHaveValue("4.000000");
  });
});

const candidate = {
  purchaseOrderId: "order-1",
  purchaseOrderNumber: "PO-2026-000001",
  warehouseId: "warehouse-1",
  warehouseCode: "MAIN",
  warehouseName: "Main warehouse",
  currencyCode: "DZD",
  version: 3,
  lines: [
    {
      purchaseOrderLineId: "line-1",
      lineNumber: 1,
      productSku: "SKU-001",
      productName: "Receipt product",
      unitOfMeasure: "EA",
      orderedQuantity: 10,
      receivedQuantity: 0,
      outstandingQuantity: 10,
      conversionFactorToBaseUnit: 1,
    },
  ],
};

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/goods-receipts/new/order-1"]}>
      <Routes>
        <Route
          element={<GoodsReceiptCreatePage />}
          path="/goods-receipts/new/:purchaseOrderId"
        />
      </Routes>
    </MemoryRouter>,
  );
}
