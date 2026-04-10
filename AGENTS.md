# Repository Guidance

## Rewrite Direction
- Treat this repository as legacy reference material and workflow tooling for the rewrite.
- The rewrite itself belongs in a fresh libGDX project, not in the current Android/AndEngine tree.
- Preserve gameplay behavior, data semantics, and compatibility expectations. Do not preserve AndEngine structure or Android-specific architecture.

## Default Workflow
- Use `scripts/oabtw-preflight` before starting a new libGDX or peer-analysis session.
- Use `scripts/oabtw-explore-peer` and `scripts/oabtw-review-peer` for legacy codebase analysis.
- Use `scripts/oabtw-architecture-peer` to synthesize analysis into rewrite architecture docs.
- Use `scripts/oabtw-implement-peer` only after the analysis/docs lane has produced concrete hotspots.

## Docs and Output
- Durable local analysis and architecture docs live under `docs/`.
- Ephemeral peer-run state lives under `.openagentsbtw/`.
- The rewrite architecture must follow official libGDX guidance first, then adapt around project-specific gameplay needs.

## Architecture Defaults
- Favor thin platform launchers and a shared `core`.
- Keep platform APIs out of shared gameplay code.
- Favor explicit subsystem boundaries, deterministic gameplay logic, and small cohesive modules over god objects.
