# Codex Development Workflow

Use this process for every feature.

## 1. Define the Feature

Create a feature specification using `FEATURE_TEMPLATE.md`.

The specification must include:

- Goal
- Scope
- Out of scope
- Business rules
- Acceptance criteria
- Required tests
- Expected database changes

## 2. Ask Codex to Inspect

Prompt:

```text
Read AGENTS.md and the relevant files in /docs.

Inspect the current code related to this feature.
Do not edit anything.

Explain:
- current implementation
- relevant extension points
- risks
- missing requirements
```

## 3. Ask Codex for a Plan

Prompt:

```text
Create an implementation plan for this feature.

Include:
- database changes
- backend changes
- frontend changes
- validation
- unit tests
- integration tests
- manual tests
- files expected to change

Do not write code yet.
Keep the work limited to the approved scope.
```

Review and correct the plan before implementation.


## Branch and Pull-Request Workflow

Every approved roadmap or implementation-plan step is completed on its own branch and delivered through its own pull request.

- Create the branch before editing implementation files; do not implement a step directly on the main branch.
- Keep the branch and pull request limited to that one approved step. Do not start the next step until its pull request is merged.
- Commit continuously at meaningful, reviewable boundaries (for example: backend contract, frontend UI, tests, documentation). Do not put the whole step into one final catch-all commit.
- Each commit must be focused, buildable when practical, and use the repository commit convention.
- Before opening the pull request, run the relevant checks and describe its scope, decisions, tests, and known risks.

## 4. Implement One Slice

Prompt:

```text
Implement the approved plan.

Requirements:
- follow AGENTS.md
- use clean code and clear separation of concerns
- do not make unrelated changes
- run relevant builds and tests
- report files changed and test results
```

Codex may change entity models and EF configurations, but must stop before migration generation. The developer runs the migration commands manually, reviews their output, and applies the migration manually.

## 5. Review the Change

Prompt:

```text
Review the implemented change as a pull request.

Look for:
- incorrect business rules
- transaction problems
- concurrency problems
- EF Core issues
- missing database constraints
- missing tests
- API error-handling problems
- frontend loading and error-state problems
- unnecessary abstractions
- duplicated constants, route paths, or configuration

Do not change anything yet. Report findings first.
```

Approve only valid fixes.

## 6. Generate and Apply the Migration Manually

After reviewing model changes, the developer—not Codex—runs:

```text
dotnet ef migrations add <MeaningfulName> --project src/Warehouse.Infrastructure --startup-project src/Warehouse.Api
dotnet ef database update --project src/Warehouse.Infrastructure --startup-project src/Warehouse.Api
```

Never edit the generated migration or snapshot manually. If a migration has been applied, create a later corrective migration instead.

## 7. Manual Verification

Run the manual checklist from the feature specification.

Verify the feature through:

- Swagger or an API client
- The React UI
- PostgreSQL data where needed

## 8. Complete the Feature

A feature is complete only when:

- Acceptance criteria pass.
- Unit tests pass.
- Integration tests pass.
- Frontend tests pass where relevant.
- Backend builds.
- Frontend builds.
- Manual checks pass.
- Documentation is updated.
- The branch contains focused logical commits created during the work.
- The pull request for this step is approved and merged.

## 9. Move to the Next Feature

Before starting the next feature:

- Confirm the current branch is clean.
- Confirm all tests pass.
- Update the roadmap if scope changed.
- Create the next feature specification.