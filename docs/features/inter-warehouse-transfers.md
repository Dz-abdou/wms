# Feature: Inter-Warehouse Transfers

## Status

Approved for implementation on the `features/inter-warehouse-transfers` branch.

## Goal

Let an inventory operator move stock between two active warehouses while preserving a complete, immutable document and an auditable stock ledger.

## Scope

- An immediately completed transfer document with source warehouse, destination warehouse, optional reference/note, timestamp, and audit metadata.
- An editable create page with a shared header and an Ant Design editable line table.
- A transfer line snapshots product, selected UoM, entered quantity, converted base quantity, and its two resulting movement IDs.
- One negative `TransferOut` movement at the source and one positive `TransferIn` movement at the destination for every posted line.
- Paged transfer list, immutable detail page, navigation entry, and movement-history links back to the transfer.
- English/French loading, empty, error, validation, and success states.

## Out of Scope

- Draft, in-transit, dispatched, received, cancelled, or reversed transfer statuses.
- Bins, lots, serials, barcode scanning, transport carriers, transfer approvals, reservations, and partial receiving.
- Creating ledger movements directly or changing a posted transfer.

## Business Rules

1. A transfer has one active source warehouse and one different active destination warehouse.
2. Each product appears at most once in a transfer, and quantity must be positive in a valid product UoM.
3. The command converts the entered quantity to the product base UoM, validates sufficient source stock, then atomically applies the source decrease and destination increase.
4. Every successful transfer line creates exactly two linked inventory movements: `TransferOut` and `TransferIn`. They share the transfer ID and use the same base quantity/UoM.
5. A failure on any line, including insufficient source stock, invalid UoM, inactive master data, or a concurrency conflict, rolls back the entire document, both balances, and all movements.
6. The transfer document and its lines are immutable after posting. The first release is immediately completed; a later phase may add in-transit receipt states without changing these posted snapshots.

## API

- `GET /api/inventory/transfers?page=1&pageSize=20&sourceWarehouseId=&destinationWarehouseId=&reference=&fromUtc=&toUtc=`
- `GET /api/inventory/transfers/{id}`
- `POST /api/inventory/transfers`
- Existing `GET /api/inventory/movements` includes transfer IDs/references and links to transfer details.

## Frontend Requirements

- Inventory navigation exposes **Transfers**; its landing page is a paginated list with one **New** action.
- The create page contains source warehouse, destination warehouse, reference, and note, followed by an editable line table: product, available source quantity/base UoM, transfer UoM, quantity, and remove action.
- Source and destination must be different; changing either warehouse removes existing lines so displayed availability cannot be misleading.
- Quantity errors, including insufficient stock, appear on the exact line cell using translated stable error codes and parameters.
- The detail page displays read-only source/destination summary data and a fixed-action line table showing product, UoM, quantity, source balance after, and destination balance after.
- Every modified frontend file is formatted with the repository formatter; feature styling remains in a CSS module only if required.

## Acceptance Criteria

1. Given 10 EA of a product in Warehouse A, transferring 3 EA to Warehouse B creates one transfer, `TransferOut` of -3 EA at A, `TransferIn` of +3 EA at B, and leaves balances at 7 and 3 EA.
2. Given an insufficient source balance, posting returns a localized field error on `Lines[n].Quantity` and creates no transfer, movements, or balance changes.
3. Source and destination cannot be the same, products cannot repeat, and invalid product UoMs are rejected without side effects.
4. A movement-history row for either transfer movement links to the immutable transfer detail and shows the transfer reference when present.
5. Paged list/detail endpoints, atomic transfer behavior, field error mapping, and create/list/detail UI states are covered by automated tests.

## Database Changes

- Add `InventoryTransfers` and `InventoryTransferLines`.
- Add nullable `InventoryTransferId` to `InventoryMovements`, its index, and a restrictive foreign key.
- The developer must generate, review, and apply the EF Core migration manually. Codex does not edit migrations or the model snapshot.
