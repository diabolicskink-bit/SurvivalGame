# Write Architecture Refactor Plan Markdown

## Summary
Create `docs/ARCHITECTURE_REFACTOR_PLAN.md` containing the architectural review and refactor roadmap from the last response, formatted as a durable project planning document.

## Key Changes
- Add a new Markdown document with:
  - current architecture assessment
  - major maintainability risks
  - recommended refactor order
  - rationale for prioritizing an application layer and campaign-level command pipeline
- Do not change production code.
- Do not update `docs/TASK_LOG.md` unless this is treated as an implemented documentation task.

## Test Plan
- No code tests required.
- Verify the Markdown file is readable, scoped, and does not imply new gameplay systems are currently implemented.

## Assumptions
- Default output path: `docs/ARCHITECTURE_REFACTOR_PLAN.md`.
- The file should preserve the review as guidance, not as current scope.
