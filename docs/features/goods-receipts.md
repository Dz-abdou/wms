# Feature: Goods Receipts — Accepted Quantity Slice

## Status

Implementation complete — awaiting final manual database-constraint migration and
manual workflow verification on `features/Goods-Receipts`.

## Goal

Allow a receiving manager to record accepted delivery quantities against a
submitted purchase order. The operation must preserve the purchase-order line
snapshots, increase stock in the purchase order's destination warehouse, and
leave no partial inventory changes if any line is invalid.

## Scope

- Create a goods-receipt document against one submitted or partially received
  purchase order.
- Record received timestamp, optional supplier delivery note, optional notes,
  receiving user, and accepted quantities by purchase-order line.
- Default a receipt form from the purchase order's outstanding quantities.
- Validate each receipt line against the outstanding quantity and snapped PO
  UoM/base-UoM conversion.
- Increase inventory balances and create one linked inventory movement for
  every accepted line in a single transaction.
- Update the purchase-order status to `PartiallyReceived` or `Received` and
  append status history.
- Provide receipt list, create, and detail screens with localized loading,
  empty, error, and success states.

## Out of Scope

- Damaged, rejected, quarantined, or returned quantities.
- Over-receipt permissions or tolerance rules.
- Lots, expiry dates, serial numbers, bins, put-away, attachments, labels,
  supplier invoice matching, or quality inspection.
- Editing, cancelling, or deleting a posted receipt.
- Generic source-document modelling; receipt-specific references are explicit.

## Business Rules

1. Only a submitted or partially received purchase order may be received.
2. A receipt uses the purchase order's destination warehouse; users cannot
   redirect a receipt to another warehouse.
3. Each request line identifies one PO line and must have a positive accepted
   quantity in the PO line's snapped purchase UoM.
4. The aggregate accepted quantity across posted receipts cannot exceed the PO
   line's ordered quantity. The API returns a stable error against
   `Lines[n].AcceptedQuantity` when it would.
5. Each accepted quantity uses the PO line's immutable conversion factor to
   update inventory in base units. Later product or catalogue changes cannot
   alter a posted receipt.
6. A receipt has no editable currency or exchange-rate field. It inherits the
   purchase order's single currency and uses the PO-line commercial snapshots
   only as read-only context; receiving never converts or reprices quantities.
7. Creating a receipt, applying all balances, writing all movements, and
   changing purchase-order status occurs in one transaction. Any invalid line
   rolls back the entire receipt.
8. A receipt is immutable after posting. Its number and timestamps are
   historical records.
9. A receipt request includes the version returned with its purchase-order
   receipt candidate. Posting rechecks that version while changing the PO
   status; a stale request returns
   `goods_receipt.purchase_order_concurrency_conflict` and makes no changes.
   This prevents two receivers from over-receiving the same outstanding PO
   line. The UI preserves entered accepted quantities and asks the user to
   refresh the outstanding quantities before retrying.
10. Inventory-balance writes retain existing optimistic-concurrency protection.
    A balance conflict rolls back the receipt transaction and returns a stable
    retryable conflict code; it never produces a partial receipt.

## Data Model Changes

- `GoodsReceipt`: number, purchase-order ID, destination-warehouse ID,
  received-at UTC, supplier delivery note, notes, receiver user ID, and
  immutable receipt lines.
- `GoodsReceiptLine`: PO-line ID, line number snapshot, product/UoM snapshots,
  accepted quantity, accepted base quantity, and linked inventory-movement ID.
- `InventoryMovement`: optional goods-receipt ID and a `GoodsReceipt` movement
  type.
- Database constraints/indexes enforce positive receipt quantities, unique
  receipt numbers, and one receipt-line entry per PO line per receipt.

Codex may implement entities and EF configuration. The developer must manually
generate, review, and apply the EF Core migration; Codex must not edit migration
files or the model snapshot.

## API Requirements

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/goods-receipts` | Paged list, filterable by PO number and warehouse. |
| GET | `/api/goods-receipts/{id}` | Receipt header and immutable lines. |
| GET | `/api/purchase-orders/{id}/receipt-candidate` | Submitted PO version and lines with received/outstanding quantities. |
| POST | `/api/goods-receipts` | Atomically post a receipt. |

All failures use Problem Details with stable codes. Request-field failures are
keyed to exact fields (for example `Lines[0].AcceptedQuantity`) and translated
inline by the frontend.

## Frontend Requirements

- A receipt list follows the shared list-page standard with server-backed PO
  and warehouse filters.
- A receipt create route starts from a submitted/partially received PO and
  displays a read-only header plus editable accepted-quantity table.
- The line table shows PO line number, product/SKU, purchase UoM, ordered,
  previously received, outstanding, accepted now, and derived base quantity.
- The PO currency is shown only as read-only commercial context when line value
  is displayed. A receipt never lets users select a different currency.
- Receipt destination warehouse, PO reference, and line context are
  informational and cannot be changed.
- The create request carries the receipt candidate's PO version. On a stale
  receipt conflict, entered quantities remain visible and a localized refresh
  action reloads the latest outstanding quantities before the user retries.
- The detail route shows immutable receipt header/line snapshots and links to
  the related PO.
- Receipt-created inventory movements are visible in Movement history and link
  back to the immutable receipt detail.
- All visible copy and API errors use English/French locale keys. Every changed
  frontend file is formatted before commit.

## Acceptance Criteria

### Valid partial receipt

Given a submitted PO with an outstanding quantity of 10 EA, when a manager
posts an accepted quantity of 4 EA, then stock increases by 4 base units, one
receipt and one movement are created, and the PO becomes Partially Received.

### Valid final receipt

Given a partially received PO with 6 EA outstanding, when a manager receives
6 EA, then stock increases by 6 base units and the PO becomes Received.

### Currency integrity

Given a purchase order in a single currency, when a manager opens or posts a
receipt, then the receipt exposes that currency only as read-only context and
cannot select, convert, or reprice in another currency.

### Atomic invalid receipt

Given a receipt request with multiple lines where one exceeds outstanding
quantity, when it is submitted, then the API returns a localized field error
and creates no receipt, movement, balance change, or PO status change.

### Safe concurrent receiving

Given two receivers open the same PO receipt candidate, when one posts a
receipt first, then the other cannot post against the stale PO version. The
second request returns `goods_receipt.purchase_order_concurrency_conflict` and
creates no receipt, movement, balance change, or PO status change.

## Unit Tests

- Receipt number and line snapshot invariants.
- Positive quantities and duplicate PO-line rejection.
- Purchase-order receipt status transitions.

## Integration Tests

- Partial/final receipt updates inventory, movements, and PO status atomically.
- Over-receipt returns an exact field error with no partial persistence.
- Concurrent receipt posting detects a stale PO version without partial
  persistence.
- Concurrent balance handling and receipt number uniqueness.

## Frontend Tests

- Receipt candidate/outstanding display and accepted-quantity validation.
- Nested server error maps to the responsible quantity cell in English/French.
- A stale receipt conflict preserves entered quantities and offers a refresh.
- List/create/detail loading, empty, and error states.

## Manual Test Checklist

- [x] Generate, review, and apply the initial goods-receipt migration manually.
- [ ] Generate, review, and apply the follow-up foreign-key/check-constraint
  migration manually.
- [ ] Post a partial receipt and verify stock/movement/PO status.
- [ ] Post the final receipt and verify the PO is received.
- [ ] Attempt an over-receipt and verify no data changes.
- [ ] Verify English and French labels/errors.

## Definition of Done

- [ ] Acceptance criteria pass.
- [ ] Developer generated and applied the migration manually.
- [ ] Unit, PostgreSQL integration, and frontend tests pass.
- [ ] Backend and frontend production builds pass.
- [ ] Manual checklist passes.
