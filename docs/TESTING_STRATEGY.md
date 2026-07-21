# Testing Strategy

## Testing Goals

Tests must prove important business behavior, not only increase coverage.

## Unit Tests

Use unit tests for business rules that can run without a database.

Examples:

- SKU validation
- Invalid quantities
- Receiving more than ordered
- Reserving more than available
- Invalid document status transitions
- Shipment rules

Unit tests should be fast and isolated.

## Integration Tests

Use integration tests for:

- API endpoints
- EF Core mappings
- PostgreSQL constraints
- Transactions
- Query behavior
- Pagination
- Concurrency
- Complete business workflows

Use PostgreSQL Testcontainers.

Do not use an in-memory provider as proof that PostgreSQL behavior works.

## Frontend Tests

Use Vitest and React Testing Library for:

- Form validation behavior
- Loading states
- Error states
- Important conditional UI
- Key user interactions
- English and French translations for visible states and API error-code mapping

Do not test Ant Design internals.

## Manual Acceptance Tests

Every feature specification must include a checklist.

Example:

```text
[ ] Create a valid product.
[ ] Duplicate SKU is rejected.
[ ] Required field errors are visible.
[ ] Product appears in the list.
[ ] Product can be edited.
[ ] API failure displays a useful error.
[ ] Data remains after page refresh.
```

## Bug Fix Rule

Every fixed bug must include a regression test when practical.

## Test Commands

Expected commands:

```bash
dotnet build
dotnet test
npm run lint
npm run test
npm run build
```

The exact package manager may be npm or pnpm, but the project must use one consistently.

## Milestone Gate

Before completing a milestone:

- Run all backend tests.
- Run all frontend tests.
- Build backend and frontend.
- Run the main workflows manually.
- Verify Docker setup from a clean environment.
