# Feature: Warehouses

## Status

In progress — Phase 2

## Goal

Allow operators to maintain warehouses that later inventory workflows can reference.

## Scope

- Create, edit, view, paginate, activate and deactivate Warehouses.
- Enforce unique case-insensitive Warehouse codes.
- Provide English/French UI text and stable API error codes.

## Out of Scope

- Inventory, bins, addresses, deletion, authentication and audit history.
- Search and filtering.

## Business Rules

1. Code is required, trimmed, stored uppercase, 1–32 characters, and unique.
2. Name is required, trimmed, and 1–200 characters.
3. Description is optional and limited to 500 characters.
4. New Warehouses are active; status changes are idempotent.
5. Missing Warehouses return `warehouse.not_found`; duplicate codes return `warehouse.code_conflict`.

## Data Model

`Warehouse`: UUID ID, Code, Name, optional Description, active status, and UTC creation/update timestamps. PostgreSQL enforces nonblank, uppercase Code and unique Code constraints.

## API

- `GET /api/warehouses?page=1&pageSize=20`
- `GET /api/warehouses/{id}`
- `POST /api/warehouses`
- `PUT /api/warehouses/{id}`
- `PATCH /api/warehouses/{id}/status`

## Definition of Done

- [x] Developer generated and applied the Warehouse migration manually.
- [ ] Unit, integration and frontend tests pass.
- [ ] Manual API and UI checks pass.
- [ ] Phase 2 PR is reviewed and merged.