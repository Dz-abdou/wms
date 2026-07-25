# Feature: Inventory Adjustment Documents and Ledger

## Status

Approved for implementation on the current supplier/purchasing branch by developer direction. This slice completes the existing batch-adjustment workflow with document navigation and a usable read-only movement ledger. It does not introduce a new database entity or migration.

## Goal

Present manual stock adjustments as immutable business documents and inventory movements as a general, read-only ledger.

## Scope

- Paged adjustment-document list with a **New adjustment** action.
- Separate adjustment create route and immutable adjustment-detail route.
- Adjustment detail header: reason, reference, note, created timestamp; line table is derived from its linked movements.
- General paged movement-history list, with optional product, warehouse, movement-type, date, and adjustment-reference filters.
- Movement rows include product and warehouse display context and a link to their adjustment document when applicable.
- A successful adjustment routes to its detail screen.
- English/French navigation, loading, empty, error, and success states.

## Out of Scope

- Editing, deleting, or reversing a posted adjustment.
- New inventory tables, schema changes, transfers, counts, goods receipts, reservations, lots, or a generic source-document model.
- Changes to adjustment posting, stock-balance rules, or the required atomic transaction.

## Business Rules

1. A posted adjustment is immutable. Users create it through the existing batch command; they do not create inventory movements directly.
2. Every adjustment line has exactly one existing movement linked by `InventoryAdjustmentId`.
3. Movement history is read-only. Its filters refine the ledger; they must not be required before a user can see records.
4. Operators may read the ledger and adjustment documents; only users with adjustment permission may create an adjustment.

## API

- `GET /api/inventory/adjustments?page=1&pageSize=20`
- `GET /api/inventory/adjustments/{id}`
- `GET /api/inventory/movements?page=1&pageSize=20&productId=&warehouseId=&type=&fromUtc=&toUtc=&reference=`
- Existing `POST /api/inventory/adjustments` remains the sole stock-changing action.

## Acceptance Criteria

1. Inventory navigation exposes **Movement history** and **Adjustments** separately.
2. The Adjustments screen is a paginated list with a **New adjustment** action; the form is not the menu landing page.
3. Submitting a valid adjustment opens its immutable detail screen, which shows all resulting movement lines.
4. The ledger opens with all movements and supports optional filters, pagination, and readable product/warehouse context.
5. An unknown adjustment returns `inventory.adjustment_not_found` and the UI displays localized feedback.
6. Existing adjustment transaction and movement-creation behavior remains unchanged.

## Tests

- Integration tests: adjustment list/detail, ledger filters, document/movement linkage, and unknown adjustment code.
- Frontend tests: ledger loading/empty/filter states, adjustment list action, create-to-detail navigation, and adjustment detail rendering.
- Full backend and frontend suites before handoff.

## Database Changes

None. Existing `InventoryAdjustments`, `InventoryMovements.InventoryAdjustmentId`, and its index supply the required persistence relationship.
