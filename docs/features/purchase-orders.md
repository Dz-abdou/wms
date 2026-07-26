# Feature: Purchase Orders

## Status

Core implementation exists on the combined `features/Suppliers-+-Purchase-Orders` branch by explicit developer decision. It remains subject to migration, verification, review, and merge before it is considered complete.

This document is deliberately limited to the core Phase 4 purchase-order slice. Once it is complete, the next work is [Phase 4.1 — Purchase Order Operational Hardening](../ROADMAP.md#phase-41--purchase-order-operational-hardening), guided by [Priority 1 of the Operational WMS Plan](../OPERATIONAL_WMS_PLAN.md#priority-1--purchase-order-hardening). Do not expand this core slice with those follow-up requirements without an approved implementation plan.

## Goal

Allow purchasing managers to maintain supplier-specific product catalogue terms, create draft purchase orders, and submit immutable orders for later goods-receipt workflows.

## Scope

- Manage active supplier catalogue entries for a supplier, product, and valid purchase UoM.
- Store the current effective unit price, ISO currency, supplier SKU, and minimum order quantity for each catalogue entry.
- Use the backend-owned allowed-currency catalogue; DZD is the initial default and EUR/USD are enabled initially. The supplier catalogue stores the selected ISO code, not arbitrary free text.
- Restrict each purchase-unit choice to the selected product's base UoM or a configured product conversion.
- Create, view, list, edit, and submit purchase orders.
- Snapshot catalogue product, UoM, price, currency, and supplier SKU data onto each purchase-order line.
- Enforce `Draft` and `Submitted` statuses; submitted orders are immutable.

## Out of Scope

- Supplier price history, price breaks, tax, discounts, payment terms, approval workflow, cancellation, attachments, and delivery dates.
- Receiving warehouse selection, goods receipts, inventory changes, and purchase-order completion.
- Deleting supplier catalogue entries or purchase orders.

### Explicit Phase 4.1 Follow-up

Phase 4.1 owns the human-readable PO number, destination warehouse, header currency, order and expected-delivery dates, buyer, notes, concurrency token, submitted timestamp, line number, immutable base-unit conversion/quantity snapshots, status history, cancellation, totals, and the richer operational list/detail UI. Goods receipts remain Phase 5 and must not be implemented as part of this specification.

## Business Rules

1. A catalogue entry is unique per supplier, product, and purchase UoM. A product may have entries from many suppliers and one supplier may offer multiple purchase UoMs for one product.
2. Its purchase UoM must be the product base unit or one of its defined conversions. Minimum order quantity is positive and respects the UoM's fractional-quantity rule.
3. Prices are non-negative decimal values in one of the backend-owned allowed ISO currency codes. They are current effective values and are copied to draft lines.
4. An order supplier and every selected catalogue entry must be active. The referenced product must also be active.
5. A draft can be created and edited. It can be submitted only with at least one distinct catalogue line, each at or above its minimum order quantity.
6. Submitted orders cannot have their supplier or lines changed. Future goods receipts may change their status only through an explicit transition.

## API

### Supplier catalogue

- `GET /api/supplier-products?page=1&pageSize=20&supplierId=...&productId=...`
- `GET /api/supplier-products/{id}`
- `POST /api/supplier-products`
- `PUT /api/supplier-products/{id}`
- `PATCH /api/supplier-products/{id}/status`

### Purchase orders

- `GET /api/purchase-orders?page=1&pageSize=20&status=...`
- `GET /api/purchase-orders/{id}`
- `POST /api/purchase-orders`
- `PUT /api/purchase-orders/{id}`
- `PATCH /api/purchase-orders/{id}/submit`

## Acceptance Criteria

1. A manager can add an active catalogue item with a valid product UoM and current price.
2. Creating a draft with a selected supplier snapshots the catalogue terms to its lines.
3. Invalid catalogue entries, inactive records, duplicate lines, and quantities below MOQ are rejected with stable error codes.
4. A non-empty draft submits successfully; subsequent edits receive a stable immutable-order error.
5. Operators can read catalogues and purchase orders; managers can manage and submit them.
6. The catalogue form offers only product-defined purchase units and centrally configured allowed currencies; the API independently enforces both rules.

## Tests

- Domain unit tests cover UoM validation, line snapshots, MOQ, and draft/submitted state rules.
- PostgreSQL integration tests cover constraints, authorization, API error codes, and immutable submitted orders.
- Frontend tests cover catalogue and PO loading, validation, submission, and localized errors.

## Manual Test Checklist

- [ ] Generate and review the migration after model changes.
- [ ] Add a supplier catalogue entry in the UI.
- [ ] Create, edit, submit, and view a draft purchase order.
- [ ] Verify submitted orders cannot be edited through the API or UI.
- [ ] Verify English/French text and error feedback.

## Handoff

After this core slice is fully verified and merged, create or refresh a dedicated Phase 4.1 feature specification before coding. It must use the Phase 4.1 roadmap exit criteria and the Operational WMS Plan Priority 1 acceptance criteria; it must also define exact migration, API, UI, concurrency, domain, integration, and frontend test work.
