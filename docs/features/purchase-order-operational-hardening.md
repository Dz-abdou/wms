# Feature: Purchase Order Operational Hardening

## Status

In progress — Phase 4.1 on `features/purchase-order-hardening`.

## Goal

Turn a draft purchase order into a reliable inbound business document that can
later support partial goods receipts without its supplier, quantity, UoM, or
commercial terms changing underneath it.

## Scope

- Add a human-readable, immutable purchase-order number in the
  `PO-YYYY-######` format.
- Require one destination warehouse, header currency, order date, and buyer
  for every purchase order. Expected delivery date, supplier reference, and
  notes are optional.
- Add optimistic concurrency for draft edits.
- Store line numbers, base-UoM conversion and quantity snapshots, line amounts,
  and submitted-order commercial snapshots.
- Add Draft, Submitted, PartiallyReceived, Received, and Cancelled statuses,
  together with append-only status history.
- Add totals and operational context to the purchase-order list and detail
  screens, including supplier, status, date-range, and warehouse filters.
- Update the route-level draft form to edit the required header and line table.

## Out of Scope

- Goods receipt creation, inventory changes, received quantities, or automatic
  transitions to PartiallyReceived/Received. Those belong to Phase 5.
- Supplier price history, tax, discounts, payment terms, approvals, attachments,
  CSV export, or accounting integration.
- A generic document-number subsystem. The sequence is purchase-order specific
  until another document type has the same proven need.

## Business Rules

1. A purchase order has exactly one active supplier, destination warehouse, and
   header currency. Every selected catalogue line must use that header currency.
2. A purchase order receives its immutable number at creation. The number is
   unique and generated safely when concurrent drafts are created.
3. The authenticated user is stored as the buyer. Purchasing actions require an
   authenticated user.
4. Draft edits require the current version. A stale update returns
   `purchase_order.concurrency_conflict` and never overwrites a newer draft.
5. Submission requires at least one distinct, valid catalogue line and records
   the submission timestamp and a Draft-to-Submitted history entry.
6. Submission snapshots each line's product and supplier identifiers, product
   SKU/name, supplier SKU, purchase UoM, conversion to base UoM, ordered
   quantities in both units, price, currency, and line amount. Later catalogue
   or product changes cannot change those values.
7. A submitted purchase order is not editable. Draft cancellation and submitted
   cancellation are explicit state transitions with a history entry; receipt
   workflows own transitions from Submitted to PartiallyReceived or Received.
8. Status history is append-only and records previous status, new status, time,
   acting user, and optional reason.
9. List queries filter before counting and pagination. No paged list uses
   in-memory table filtering.

## Data Model Changes

- `PurchaseOrder`: number, destination warehouse ID, currency code, order date,
  expected delivery date, buyer user ID, supplier reference, notes, submitted
  timestamp, version, and status-history collection.
- `PurchaseOrderLine`: line number, conversion factor, ordered base quantity,
  and line amount. Existing product/catalogue/commercial fields remain
  snapshots.
- `PurchaseOrderStatusHistory`: purchase-order ID, previous/new status,
  changed time, actor user ID, and optional reason.
- A purchase-order-specific sequence record supports safe yearly number
  allocation. Database constraints enforce valid statuses, positive quantities,
  non-negative prices/amounts, uppercase currency, unique order numbers, and
  unique line numbers per order.
- EF Core migrations and the model snapshot are developer-owned. The developer
  must generate, review, and apply the migration after the model changes.

## API Requirements

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/purchase-orders` | Paged operational list with supplier, status, warehouse, and date filters. |
| GET | `/api/purchase-orders/{id}` | Detail including header, totals, immutable lines, and status history. |
| POST | `/api/purchase-orders` | Create a draft with the required header and lines. |
| PUT | `/api/purchase-orders/{id}` | Replace a draft header and lines when the supplied version matches. |
| PATCH | `/api/purchase-orders/{id}/submit` | Submit a valid draft with its current version. |
| PATCH | `/api/purchase-orders/{id}/cancel` | Cancel an allowed order state with an optional reason and current version. |

All validation errors use existing Problem Details field/error-code conventions.
New stable purchase-order codes include `purchase_order.concurrency_conflict`,
`purchase_order.currency_mismatch`, `purchase_order.invalid_transition`, and
`purchase_order.warehouse_not_found` where applicable.

## Frontend Requirements

- The draft create/edit page has a header form followed by the existing editable
  line table. The header contains supplier, destination warehouse, currency,
  order date, expected delivery date, supplier reference, and notes.
- The line table shows catalogue item, product, supplier SKU, UoM, MOQ,
  quantity, unit price, currency, derived line amount, and remove action. Its
  add action stays in the table footer.
- The detail page shows order number, status, supplier, destination warehouse,
  buyer, dates, reference/notes, totals, immutable line table, and status
  timeline.
- The list shows number, supplier, status, order date, expected delivery,
  destination warehouse, line count, and total/currency. It uses the shared
  URL-backed list toolbar and server-backed supplier/warehouse selectors.
- Loading, empty, error, validation, and success states remain localized in
  English and French. Feature API modules own endpoint/query serialization and
  TanStack Query keys.

## Acceptance Criteria

### Operational document

Given a manager creates a valid draft, when it is saved, then it has a unique
`PO-YYYY-######` number, buyer, destination warehouse, header currency, order
date, and version.

### Stable submitted terms

Given a valid draft is submitted, when its catalogue price or product conversion
changes later, then the submitted purchase order still exposes its original line
snapshots, base quantities, currency, amounts, and total.

### Safe concurrent draft editing

Given two users open the same draft, when one saves first and the other saves
with the old version, then the second receives
`purchase_order.concurrency_conflict` and the first update remains intact.

### Lifecycle visibility

Given an order is submitted or cancelled, when its detail page is viewed, then
the current status and append-only status timeline are displayed.

## Tests

### Unit Tests

- Number, header, line-number, total, conversion/quantity, and transition
  invariants.
- Submitted-order immutability and stale-version detection.

### Integration Tests

- Unique concurrent number allocation and database constraints.
- Draft update concurrency conflict, status history, filters before pagination,
  and immutable submitted snapshots.
- Stable Problem Details error codes and purchasing authorization boundaries.

### Frontend Tests

- Header/line validation, version serialization, and translated server errors.
- List query serialization, totals/detail timeline rendering, and draft versus
  submitted action availability.

## Manual Test Checklist

- [ ] Generate and review the migration manually.
- [ ] Create, edit, submit, and cancel a purchase order.
- [ ] Verify the number, header, totals, and timeline in English and French.
- [ ] Attempt a stale draft save and verify localized conflict feedback.
- [ ] Change catalogue/product data after submission and verify the PO remains
      unchanged.
- [ ] Verify list filters, pagination, and return navigation.

## Definition of Done

- [ ] Acceptance criteria pass.
- [ ] Developer generated and applied the migration manually.
- [ ] Unit, PostgreSQL integration, and frontend tests pass.
- [ ] Backend and frontend production builds pass.
- [ ] Manual checklist passes.
- [ ] Goods receipts and inventory changes remain out of scope.
