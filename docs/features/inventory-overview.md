# Feature: Inventory Overview

## Status

Approved for implementation on the `features/inventory-operations` branch. This is the first new inventory-control vertical slice after the completed adjustment-document, movement-ledger, and goods-receipt work.

## Goal

Give inventory operators a fast, read-only view of on-hand stock by product and warehouse, with an immediate drill-down to the related movement ledger.

## Scope

- Server-paged inventory-balance overview backed by the existing `InventoryBalances` records.
- Product SKU/name search, warehouse, product category, and product active-status filters.
- Product and warehouse context, on-hand quantity in the product base UoM, and the balance's last-updated timestamp.
- A row action that opens Movement history already filtered to that product and warehouse.
- English/French loading, empty, error, and populated states, URL-backed filters, and a horizontally contained table.

## Out of Scope

- Reservations, available stock, incoming stock, low-stock thresholds, or reorder planning.
- Products with no existing balance record, stock changes, adjustments, cycle counts, transfers, bins, lots, or expiry handling.
- New tables, fields, indexes, or EF Core migrations.

## Business Rules

1. The overview is read-only and never changes a balance directly; every stock change continues to go through an immutable inventory movement.
2. Each row represents one product/warehouse balance. Quantity is shown in the product's base unit of measure.
3. Search and filters execute on the server before pagination. The UI must not filter only a loaded page.
4. A ledger drill-down preserves the selected product and warehouse in the movement-history URL.

## API Requirements

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/inventory/overview?page=1&pageSize=20&search=&warehouseId=&categoryId=&isActive=` | Return paged on-hand balances with product and warehouse display context. |

## Frontend Requirements

- `/inventory` is the Inventory overview and is the Inventory navigation landing page.
- Use the shared list layout, URL-query pagination/filter helpers, and the common clear-filters action.
- Searchable warehouse and category selectors fetch their options server-side.
- The table has product, warehouse, on-hand, last updated, and action columns, with an explicit `scroll.x` containment value.

## Acceptance Criteria

1. An operator can open Inventory and see every existing balance they are authorized to read, paged by the server.
2. SKU/name search and warehouse, category, and active-status filters return the correct subset across pages.
3. A row's on-hand quantity and base UoM match the persisted balance and product context.
4. Selecting **View history** opens the read-only movement ledger filtered to that row's product and warehouse.
5. Empty, loading, and API-error states are localized in English and French.

## Tests

- Integration: overview returns product/warehouse context, filters server-side, paginates, and excludes nonmatching balances.
- Frontend: overview renders populated and empty states, updates filters, and creates the correct ledger drill-down link.
- Run the relevant backend integration tests, frontend suite, lint, and production build before handoff.

## Database Changes

None. The existing `InventoryBalances` table and its unique product/warehouse index supply this read model.
