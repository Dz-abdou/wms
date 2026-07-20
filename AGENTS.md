# Codex Project Instructions

## General Working Rules

- Read this file and the relevant documents in `/docs` before making changes.
- Inspect the existing implementation before proposing a solution.
- For non-trivial work, provide a plan before editing files.
- Implement one vertical slice at a time.
- Do not add unrelated features or refactor unrelated code.
- Do not introduce a dependency without explaining why it is needed.
- Prefer simple, readable code over unnecessary abstractions.
- Never claim a task is complete without running relevant checks.
- At the end of a task, report:
  - files changed
  - important decisions
  - tests executed
  - remaining risks or TODOs

## Backend Rules

- Use ASP.NET Core Web API, EF Core and PostgreSQL.
- Keep API endpoints thin.
- Business rules must not live in controllers.
- Organize code by feature where practical.
- Do not add a generic repository over EF Core.
- Use dependency injection.
- Use async database operations.
- Pass cancellation tokens through application and database calls.
- Use Problem Details for API errors.
- Validate input on the server.
- Use UTC for stored timestamps.
- Use decimal for monetary values.
- Use database constraints for rules that the database can enforce.
- Use explicit transactions for multi-step inventory changes.
- Use optimistic concurrency for inventory balances.
- Never edit an already-applied migration; create a new migration.
- Every inventory quantity change must create an inventory movement.

## Frontend Rules

- Use React, TypeScript and Vite.
- Use TanStack Query for API server state.
- Use Ant Design as the default UI component library.
- Use Ant Design Table for standard business tables.
- Use TanStack Table only when custom headless table behavior is required.
- Do not mix multiple UI systems without a clear reason.
- Prefer shared API hooks by feature.
- Every page must handle loading, empty, error and success states.
- Forms must display server validation errors.
- Keep business logic out of presentation-only components.
- Use strict TypeScript settings.
- Do not use `any` unless documented and unavoidable.

## Testing Rules

- Business rules require unit tests.
- API and persistence workflows require integration tests.
- Use PostgreSQL Testcontainers for database integration tests.
- Do not mock EF Core DbContext for persistence tests.
- Every bug fix requires a regression test.
- Run relevant tests after each change.
- Run the complete test suite before completing a milestone.

## Scope Control

Before implementation, confirm:

- What is included?
- What is explicitly excluded?
- What are the acceptance criteria?
- Which business rules apply?
- Which tests prove completion?

If requirements are unclear, state assumptions in the plan instead of silently inventing behavior.
