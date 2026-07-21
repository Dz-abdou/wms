# Codex Project Instructions

## General Working Rules

- Read this file and the relevant documents in `/docs` before making changes.
- Inspect the existing implementation before proposing a solution.
- For non-trivial work, provide a plan before editing files.
- Implement one vertical slice at a time.
- Use one dedicated branch and pull request for each approved roadmap or implementation-plan step; do not start the next step before that pull request is merged.
- Create focused logical commits as work progresses (such as contract, UI, tests, and documentation); never defer all work into one catch-all commit at the end.
- Do not add unrelated features or refactor unrelated code.
- Do not introduce a dependency without explaining why it is needed.
- Prefer simple, readable code over unnecessary abstractions.
- Never claim a task is complete without running relevant checks.
- At the end of a task, report:
  - files changed
  - important decisions
  - tests executed
  - remaining risks or TODOs

## Clean Code and Design Rules

- Apply SOLID principles pragmatically: use single-purpose types, explicit dependencies, and narrow interfaces only when they solve a real boundary.
- Separate concerns: domain rules belong in Domain, use cases and contracts in Application, persistence in Infrastructure, HTTP translation and error mapping in API, and rendering in frontend components.
- Keep methods focused and short enough to understand without hidden side effects. Prefer clear names over comments that restate code.
- Remove duplication when it represents one rule or concept. Do not create abstractions merely to remove a few similar lines.
- Avoid magic numbers, URLs, route fragments, field limits, query keys, and reusable messages. Put shared feature constants in one named location.
- Do not hardcode environment-specific configuration, credentials, hostnames, or API origins. Use configuration or environment variables with documented development defaults.
- Avoid premature optimization. Measure first; use stable query keys, targeted cache invalidation, server-side pagination, and memoization only where they have a clear purpose.
- Keep dependencies current and supported. Before adding or upgrading a package, verify the current compatible version and explain its purpose. Prefer built-in platform features when sufficient.

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
- Every API error must include a stable machine-readable error code. Do not make frontend behavior depend on English or French error messages.
- Keep error-code names feature-oriented and documented, for example `product.sku_conflict` or `validation.required`.
- Validate input on the server.
- Use UTC for stored timestamps.
- Use decimal for monetary values.
- Use database constraints for rules that the database can enforce.
- Use explicit transactions for multi-step inventory changes.
- Use optimistic concurrency for inventory balances.
- Never edit an already-applied migration; create a new migration.
- Codex must never generate, modify, remove, or edit EF Core migration files or model snapshots. After model changes, stop and give the user the exact `dotnet ef migrations add` and `dotnet ef database update` commands to run manually.
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
- Support English and French from the first user-facing feature. All user-visible frontend text must use translation keys from JSON locale files; do not hardcode display text in components.
- Display backend errors by translating their stable error codes locally. Backend messages are diagnostic fallbacks, not UI copy.
- Forms must display server validation errors.
- Keep business logic, API calls, and state orchestration out of presentation-only components; use feature hooks and API modules.
- Centralize feature constants, query keys, endpoint paths, validation limits, and formatting helpers. Do not scatter magic values through pages or components.
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