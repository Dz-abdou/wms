# Feature: Purchase Orders

## Status

In progress — Phase 4, implemented on the Supplier branch at the developer's request

## Goal

Allow purchasing managers to maintain supplier-specific product catalogue terms, create draft purchase orders, and submit immutable orders for later goods-receipt workflows.

## Scope

- Manage active supplier catalogue entries for a supplier, product, and valid purchase UoM.
- Store the current effective unit price, ISO currency, supplier SKU, and minimum order quantity for each catalogue entry.
- Create, view, list, edit, and submit purchase orders.
- Snapshot catalogue product, UoM, price, currency, and supplier SKU data onto each purchase-order line.
- Enforce `Draft` and `Submitted` statuses; submitted orders are immutable.

## Out of Scope

- Supplier price history, price breaks, tax, discounts, payment terms, approval workflow, cancellation, attachments, and delivery dates.
- Receiving warehouse selection, goods receipts, inventory changes, and purchase-order completion.
- Deleting supplier catalogue entries or purchase orders.

## Business Rules

1. A catalogue entry is unique per supplier, product, and purchase UoM. A product may have entries from many suppliers and one supplier may offer multiple purchase UoMs for one product.
2. Its purchase UoM must be the product base unit or one of its defined conversions. Minimum order quantity is positive and respects the UoM's fractional-quantity rule.
3. Prices are non-negative decimal values with a required three-letter uppercase currency code. They are current effective values and are copied to draft lines.
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
