# How to Start This Project with Codex

## Current Project Continuation

This repository is beyond the Phase 0 bootstrap prompts below. For every new task, read these documents in order before proposing or changing code:

1. `AGENTS.md` — mandatory engineering, testing, migration, and branch rules.
2. `docs/PROJECT_BRIEF.md` and `docs/ARCHITECTURE.md` — product boundaries and technical design.
3. `docs/ROADMAP.md` — the authoritative delivery sequence and current next phase.
4. `docs/OPERATIONAL_WMS_PLAN.md` — the authoritative business decisions, data relationships, fields, screen expectations, and business rules that refine the roadmap.
5. The relevant file in `docs/features/` — the delivery contract for the current vertical slice: exact scope, exclusions, API expectations, acceptance criteria, and tests.
6. `docs/DEVELOPMENT_WORKFLOW.md`, `docs/TESTING_STRATEGY.md`, and `docs/LOCALIZATION_AND_ERROR_CONTRACT.md` — process and cross-cutting rules.

### How to Resolve Documentation Differences

- User instructions and `AGENTS.md` always take precedence.
- `ROADMAP.md` decides **which phase is next**.
- `OPERATIONAL_WMS_PLAN.md` decides **what the business needs** for that phase and must be used to strengthen the next feature specification.
- The approved current feature specification decides **what is implemented in one branch/PR**. Do not implement an entire roadmap phase merely because it is described in the operational plan.
- If these documents conflict, stop before coding, state the conflict, and update the affected plan/specification with the developer's approval.

### Current Handoff

The current combined supplier and core purchase-order implementation is on `features/Suppliers-+-Purchase-Orders`; combining these two slices was an explicit developer exception to the usual one-step-per-branch rule. Its generated EF Core purchasing migration is deliberately developer-owned and must not be edited by Codex.

After that branch is fully reviewed, manually migrated, verified, and merged, the next implementation slice is **Phase 4.1 — Purchase Order Operational Hardening**. It adds the operational fields and immutable snapshots required before Phase 5 Goods Receipts. Do not begin goods receipts, customers, sales orders, or shipping before Phase 4.1 meets its exit criteria.

Use this continuation prompt for a new implementation task:

```text
Read AGENTS.md and the planning documents in the order specified by README_FOR_CODEX.md.

Inspect the current branch and the code relevant to the next approved slice.
Do not edit code yet.

Report the implemented state, the gap against the relevant feature specification and
Operational WMS Plan, assumptions, risks, and a one-vertical-slice implementation plan.
State which exact acceptance criteria and tests will prove completion.
```

## Historical Bootstrap Prompts

The prompts below are retained only as the historical procedure for a brand-new Phase 0 repository. Do not use them to restart this established project.

Give Codex access to this repository, then begin with this prompt:

```text
Read AGENTS.md and every Markdown file in /docs.

Do not write code yet.

Review the project scope, architecture and roadmap.
Identify contradictions, missing foundation decisions and unnecessary complexity.

Then propose a detailed implementation plan for Phase 0 only.

The plan must include:
- repository structure
- .NET projects
- frontend structure
- dependencies
- Docker Compose
- PostgreSQL configuration
- test setup
- CI setup
- exact verification commands

Do not start Products, Warehouses or Inventory.
Wait for approval before editing files.
```

After approving the plan, use:

```text
Implement the approved Phase 0 plan.

Follow AGENTS.md.
Do not add business entities yet.
Run all available build and test commands.
Report:
- files created or modified
- dependencies added
- commands executed
- results
- remaining issues
```

After Phase 0 is verified, copy `FEATURE_TEMPLATE.md` and create the specification for Products.
