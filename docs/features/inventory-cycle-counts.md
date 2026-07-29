# Feature: Inventory Cycle Counts

## Status

Approved for implementation on the `features/inventory-cycle-count` branch. This is the next Priority 4 vertical slice after the completed inventory overview, movement ledger, and adjustment documents.

## Goal

Let an inventory operator record the physically counted quantity of products in one warehouse, retain the system/count/variance evidence, and apply the resulting stock correction atomically.

## Scope

- Immutable cycle-count document with one warehouse, optional reference/note, counted timestamp, and audit metadata.
- Editable create page: choose a warehouse, add distinct product lines, view the captured system quantity, choose a valid product UoM, and enter the physical counted quantity.
- The API creates an adjustment movement only for a non-zero variance. A matching count remains a documented count line but creates no zero-quantity movement.
- Count list and read-only detail pages, plus links from resulting ledger movements back to their count document.
- Server-side product search and candidate lookup; URL-backed list filters and paginated results.
- English/French loading, empty, error, success, and field-validation states.

## Out of Scope

- Scheduled count plans, blind counts, count assignments, approvals, recounts, batches, bins, lots, serials, or barcode scanning.
- Transfers, reservations, available-to-promise, costing, low-stock/reorder planning, editing, cancelling, or reversing a posted count.
- A generic source-document abstraction. Cycle counts are an explicit movement source in this release.

## Business Rules

1. A cycle-count document belongs to exactly one active warehouse and is immutable once posted.
2. Each product may occur once in a count. Its one-based line number is stable within the document.
3. A line snapshots the system on-hand quantity and balance version when the operator loads it. The count submits the physical quantity in a valid product UoM; the backend converts and persists the base quantity and variance.
4. Posting rechecks the balance version. If stock changed after the snapshot, return a field error at `Lines[n].SystemQuantityInBase` and make no changes. The operator must reload that line before posting.
5. A positive variance creates `CycleCountIncrease`; a negative variance creates `CycleCountDecrease`. A zero variance creates no movement.
6. Creating the document, lines, all changed balances, and all inventory movements is one transaction. Any failing line rolls back the full count.
7. Inactive products and warehouses cannot be counted. Counts cannot drive stock below zero because a physical count is always non-negative; the resulting balance equals the captured physical quantity.

## Data Model Changes

- `CycleCount`: warehouse ID, optional reference/note, counted-at UTC, audit metadata.
- `CycleCountLine`: cycle-count ID, stable line number, product ID, system quantity/version snapshot, counted UoM/quantity/base quantity, variance quantity, and optional resulting movement ID.
- `InventoryMovement`: optional cycle-count ID and `CycleCountIncrease` / `CycleCountDecrease` types.
- Database constraints/indexes: one product per count, unique line number per count, non-negative count quantities, line-number positivity, and foreign-key/index relationships.

Codex implements entity and configuration changes only. The developer must manually generate, review, and apply the EF Core migration; Codex must never edit migrations or the model snapshot.

## API Requirements

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/inventory/cycle-counts?page=1&pageSize=20&warehouseId=&reference=&fromUtc=&toUtc=` | Return a paged count-document list. |
| GET | `/api/inventory/cycle-counts/{id}` | Return an immutable count document and its lines. |
| GET | `/api/inventory/cycle-counts/candidate?warehouseId=&productId=` | Return the active product's current on-hand/base-UoM balance and version for one count line. |
| POST | `/api/inventory/cycle-counts` | Atomically post a cycle count. |

All failures use Problem Details with stable error codes. Field errors use exact nested request properties and localized `errorParameters` where a message needs captured system context.

## Frontend Requirements

- Inventory navigation exposes **Cycle counts**; the list is its landing route, with one primary **New** action.
- The create page has a warehouse/reference/note header and an editable line table: line number, product, system quantity, base UoM, counted UoM, counted quantity, variance, and remove action.
- The warehouse becomes read-only after a line is added; changing it requires removing lines first.
- Selecting a product retrieves and displays the current candidate snapshot. A stale line error remains beside its system-quantity cell until refreshed.
- The detail page shows immutable header/line snapshots and links to movement history. Movement history links a cycle-count movement back to its count detail.
- All changed frontend files are formatted with the repository formatter.

## Acceptance Criteria

1. Given a system balance of 10 EA and a physical count of 7 EA, posting creates one count line, one `CycleCountDecrease` movement of -3 EA, and leaves the balance at 7 EA.
2. Given a system balance of 10 EA and a physical count of 10 EA, posting preserves the balance, records the count line, and creates no zero movement.
3. Given multiple lines where one candidate balance changed after it was loaded, posting returns an inline stale-line error and persists no document, line, movement, or balance update.
4. The count detail preserves the system, physical, and variance quantities used at posting even when inventory changes later.
5. Movement history renders cycle-count movements as signed, color-coded deltas and links them to the relevant count document.

## Tests

- Unit: cycle-count and line invariants; movement direction from variance.
- Integration: variance posting, zero variance, candidate staleness, atomic rollback, linkage, list/detail filters, and stable field error codes.
- Frontend: list/create/detail states, candidate display, duplicate-line prevention, field-error mapping, and movement link.

## Manual Test Checklist

- [ ] Generate, review, and apply the cycle-count migration.
- [ ] Post a negative variance and verify count detail, balance, and red movement-history delta.
- [ ] Post a positive variance and verify the green movement-history delta.
- [ ] Post an exact count and verify no movement is created.
- [ ] Change stock after loading a count line and verify the localized stale-line error.
- [ ] Verify English/French labels and errors.
