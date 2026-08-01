# Feature: Identifier and Document Numbering Foundation

## Status

In progress — `features/document-numbering/counter-foundation`.

## Goal

Apply one consistent, concurrency-safe document-numbering foundation across existing operational documents before Sales Orders introduce another document flow.

## Scope

- Define centrally seeded document-number definitions for Purchase Orders (`PO`), Goods Receipts (`GR`), Inventory Adjustments (`IA`), Cycle Counts (`CC`), and Inventory Transfers (`TR`).
- Maintain one annual number series per document definition and allocate the next number atomically inside the caller's database transaction.
- Replace the current PO and Goods Receipt hard-coded sequence types while retaining their public format: `PO-YYYY-######` and `GR-YYYY-######`.
- Add immutable, required, globally unique document numbers to inventory adjustments, cycle counts, and transfers: `IA-YYYY-######`, `CC-YYYY-######`, and `TR-YYYY-######`.
- Retain each existing optional `Reference` field as an external/user reference; it does not become a document number.
- Expose generated numbers consistently in document API/list/detail screens and relevant movement-history context.
- Return stable, localized errors if a required document-number definition is missing, inactive, or cannot allocate a valid number.
- Cover number allocation, uniqueness, annual reset, transactional rollback/gap behaviour, and UI display with unit, PostgreSQL integration, and frontend tests.

## Out of Scope

- Administration UI for configuring definitions, prefixes, widths, or manual overrides.
- Company, site, legislation, fiscal, or statutory gap-free numbering.
- Manual document-number overrides.
- Sales Orders, Shipments, returns, or their number definitions. Their definitions will be added with the corresponding business feature.
- Changing master-data code policy or renaming domain `Name` properties.

## Identifier Rules

| Kind | Rule |
| --- | --- |
| Master data | Required unique manual code plus readable name/designation. |
| Operational document | Required immutable system-generated number. |
| External reference | Optional non-unique reference supplied by a user/external party. |
| Document line | Stable one-based line number, unique within its parent. |

## Data Model

- `DocumentNumberDefinition`: stable code, display description, prefix, digit count, reset period, active state, and manual-number policy. The initial definitions are seeded with automatic-only annual numbering.
- `DocumentNumberSeries`: definition code, annual period, next value, and a unique definition/period key. The persistence implementation uses an atomic PostgreSQL upsert/increment rather than trusting in-memory allocation.
- `PurchaseOrder.Number` remains application-required and database-unique. Making its legacy nullable column database-required is deliberately deferred to the later data-hardening migration, so existing deployments can be assessed and cleaned safely.
- `InventoryAdjustment.Number`, `CycleCount.Number`, and `InventoryTransfer.Number` become required, unique, and immutable.
- Existing legacy PO/GR sequence tables/types are removed only after the common service uses the new model.

## API and UI Requirements

- Client create payloads do not supply normal document numbers.
- Create responses return the generated number.
- List, detail, movement-history, and related-document displays use the document number as the human reference; `Reference` remains separately labelled.
- Numbering-definition faults map to stable API codes and translated UI feedback. No backend diagnostic text is primary UI copy.

## Business Rules

1. A number is unique per definition and period, and the full rendered number is unique per document table.
2. Allocation is safe under concurrent document creation and occurs within the document persistence transaction.
3. The series resets at each calendar year. The first number in a new definition/year is `000001`.
4. Failed transactional work must not persist a document or its allocated series increment. Ordinary successful allocation does not promise a gap-free legal sequence.
5. An inactive or missing definition prevents document creation with a stable configuration error.
6. Number and external reference have different meanings and are never conflated in forms, contracts, filters, or movement history.

## Acceptance Criteria

1. New POs and receipts retain their visible formats and have required unique numbers.
2. New adjustment, cycle-count, and transfer documents receive the correct automatic number and retain an optional independent reference.
3. Concurrent allocations for the same type/year cannot duplicate a number.
4. The first allocation for a new year starts at `000001` for that document type.
5. A rolled-back document workflow does not leave a persisted number-series allocation.
6. API and UI show localized, meaningful failure states for unavailable numbering configuration.

## Manual Migration

Codex will change only the model/configuration. The developer must generate, review, and apply the EF Core migration; Codex must not create or edit migrations or the model snapshot.

## Tests

- Unit: definition/series validation, number formatting, and immutable document numbers.
- Integration: seeded definition availability, annual reset, concurrent allocation, rollback, legacy PO/GR format, required/unique numbers, and generated inventory document numbers.
- Frontend: generated number presentation and separated external-reference labels in English/French.
