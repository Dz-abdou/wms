import { render, screen } from "@testing-library/react";
import type { ReactNode } from "react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { i18n } from "../../../shared/i18n/i18n";
import { InventoryAdjustmentDetailPage } from "../pages/InventoryAdjustmentDetailPage";
import { CycleCountDetailPage } from "../pages/CycleCountDetailPage";
import { InventoryTransferDetailPage } from "../pages/InventoryTransferDetailPage";

const { useCycleCountMock, useInventoryAdjustmentMock, useTransferMock } =
  vi.hoisted(() => ({
    useCycleCountMock: vi.fn(),
    useInventoryAdjustmentMock: vi.fn(),
    useTransferMock: vi.fn(),
  }));

vi.mock("../api/useInventory", () => ({
  useCycleCount: useCycleCountMock,
  useInventoryAdjustment: useInventoryAdjustmentMock,
  useTransfer: useTransferMock,
}));

describe("Inventory document detail pages", () => {
  beforeEach(async () => {
    await i18n.changeLanguage("en");
  });

  it("shows a cycle count's line number without a lines card wrapper", () => {
    useCycleCountMock.mockReturnValue({
      data: {
        countedAtUtc: "2026-08-01T10:00:00Z",
        id: "count-1",
        lines: [
          {
            baseUnitOfMeasure: "EA",
            countedQuantityInBase: 5,
            countedQuantityInUnit: 5,
            countedUnitOfMeasure: "EA",
            id: "line-1",
            inventoryMovementId: "movement-1",
            lineNumber: 1,
            productId: "product-1",
            productName: "Eaches",
            productSku: "EA-001",
            systemBalanceVersion: 1,
            systemQuantityInBase: 5,
            varianceQuantityInBase: 0,
          },
        ],
        note: null,
        reference: null,
        warehouseCode: "MAIN",
        warehouseId: "warehouse-1",
        warehouseName: "Main warehouse",
      },
      error: null,
      isLoading: false,
    });

    renderPage(<CycleCountDetailPage />);

    expect(screen.getByRole("columnheader", { name: "#" })).toBeVisible();
    expect(document.querySelector(".inventory-history-card")).toBeNull();
  });

  it("shows a transfer's line number without a lines card wrapper", () => {
    useTransferMock.mockReturnValue({
      data: {
        destinationWarehouseCode: "DEST",
        destinationWarehouseId: "warehouse-2",
        destinationWarehouseName: "Destination warehouse",
        id: "transfer-1",
        lines: [
          {
            destinationBalanceAfter: 3,
            id: "line-1",
            lineNumber: 1,
            productId: "product-1",
            productName: "Eaches",
            productSku: "EA-001",
            quantityInBaseUnit: 3,
            quantityInUnit: 3,
            sourceBalanceAfter: 7,
            transferInMovementId: "movement-2",
            transferOutMovementId: "movement-1",
            unitOfMeasure: "EA",
          },
        ],
        note: null,
        reference: null,
        sourceWarehouseCode: "SOURCE",
        sourceWarehouseId: "warehouse-1",
        sourceWarehouseName: "Source warehouse",
        transferredAtUtc: "2026-08-01T10:00:00Z",
      },
      error: null,
      isLoading: false,
    });

    renderPage(<InventoryTransferDetailPage />);

    expect(screen.getByRole("columnheader", { name: "#" })).toBeVisible();
    expect(document.querySelector(".inventory-history-card")).toBeNull();
  });

  it("shows deterministic adjustment line numbers without a lines card wrapper", () => {
    useInventoryAdjustmentMock.mockReturnValue({
      data: {
        createdAtUtc: "2026-08-01T10:00:00Z",
        id: "adjustment-1",
        lines: [
          {
            balanceAfter: 3,
            createdAtUtc: "2026-08-01T10:00:00Z",
            lineNumber: 1,
            movementId: "movement-1",
            productId: "product-1",
            productName: "Eaches",
            productSku: "EA-001",
            quantityDelta: 3,
            quantityDeltaInUnit: 3,
            type: "ManualIncrease",
            unitOfMeasure: "EA",
            warehouseCode: "MAIN",
            warehouseId: "warehouse-1",
            warehouseName: "Main warehouse",
          },
        ],
        note: null,
        reason: "FoundStock",
        reference: null,
      },
      error: null,
      isLoading: false,
    });

    renderPage(<InventoryAdjustmentDetailPage />);

    expect(screen.getByRole("columnheader", { name: "#" })).toBeVisible();
    expect(document.querySelector(".inventory-history-card")).toBeNull();
  });
});

function renderPage(page: ReactNode) {
  return render(<MemoryRouter>{page}</MemoryRouter>);
}
