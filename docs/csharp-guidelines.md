# C# Guidelines

`.editorconfig` is the lint and style source of truth. This file only covers repo structure and maintenance rules.

## Structure
- Keep one primary type per file.
- Keep namespaces aligned with folders.
- Put shared game code in `src/OsuDroid.Game`.
- Keep platform-specific host code in `src/OsuDroid.App/Platforms`.
- Keep MAUI host-only code in `src/OsuDroid.App`; keep gameplay/domain/protocol code in `src/OsuDroid.Game`.

## UI Layout
- Put scene state and transitions in `Scenes`.
- Put rendering-independent frame data in `Runtime`.
- Put MonoGame drawing/input adapters in the app host or rendering-specific folders.
- Do not let a route host turn into a product-UI grab bag.
- Group reusable UI primitives by concern under `UI/Actions`, `UI/Assets`, `UI/Elements`, `UI/Geometry`, and `UI/Style`.

## Services
- Keep shared interfaces small and focused.
- Put stub or fake implementations under `Services/Stubs`.
- Split platform adapters by concern instead of grouping storage, URI launching, permissions, and native UI into one file.
- Put protocol/database compatibility code under `Compatibility`.

## Change Hygiene
- If a file starts collecting unrelated responsibilities, split it before adding more.
- Prefer explicit names and small modules over “helpful” multipurpose containers.
- Match folder moves with namespace updates in the same change so `IDE0130` stays clean.

## Architecture Audit
- Run `python3 scripts/dev/architecture_audit.py --write docs/architecture-audit.md` before adding large scenes/subsystems.
- Treat `god-file:candidate`, `too-many-methods`, and `wide-public-surface` in source files as cleanup triggers.
- Test files may exceed thresholds when they preserve regression evidence, but split them when they block maintainability.

## Runtime Paths
- Platform hosts provide roots only; shared game code derives osu!droid folders through `DroidGamePathLayout`.
- Do not concatenate `Songs/`, `Skin/`, `Scores/`, `databases/`, or `Log/` outside the path layout unless matching legacy import data.
- Keep path semantics in `src/OsuDroid.Game`; keep native root discovery in `src/OsuDroid.App/Platform` or `Platforms`.
