# Repository Guidance

## Rewrite Direction
- This repository is now the active libGDX rewrite root.
- Preserve gameplay behavior, data semantics, and compatibility expectations from the legacy analysis docs.
- Do not reintroduce AndEngine, the old Android app architecture, or Kotlin-era dependencies into the rewrite root.

## Default Workflow
- Use `scripts/oabtw-preflight` before starting a new libGDX or peer-analysis session.
- Use `scripts/oabtw-explore-peer` and `scripts/oabtw-review-peer` for the active libGDX root plus the preserved legacy dossiers in `docs/original-codebase/`.
- Use `scripts/oabtw-architecture-peer` to synthesize current implementation state and preserved analysis into rewrite architecture docs.
- Use `scripts/oabtw-implement-peer` only after the analysis/docs lane has produced concrete hotspots.

## Docs and Output
- Durable local analysis and architecture docs live under `docs/`.
- Ephemeral peer-run state lives under `.openagentsbtw/`.
- The rewrite architecture follows official libGDX guidance first, then adapts around project-specific gameplay needs.

## Architecture Defaults
- Favor thin platform launchers and a shared `core`.
- Keep platform APIs out of shared gameplay code.
- Favor explicit subsystem boundaries, deterministic gameplay logic, and small cohesive modules over god objects.
