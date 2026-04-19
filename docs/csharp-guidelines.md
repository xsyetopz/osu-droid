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

## Services
- Keep shared interfaces small and focused.
- Put stub or fake implementations under `Services/Stubs`.
- Split platform adapters by concern instead of grouping storage, URI launching, permissions, and native UI into one file.
- Put protocol/database compatibility code under `Compatibility`.

## Change Hygiene
- If a file starts collecting unrelated responsibilities, split it before adding more.
- Prefer explicit names and small modules over “helpful” multipurpose containers.
- Match folder moves with namespace updates in the same change so `IDE0130` stays clean.
