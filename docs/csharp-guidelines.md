# C# Guidelines

`.editorconfig` is the lint and style source of truth. This file only covers repo structure and maintenance rules.

## Structure
- Keep one primary type per file.
- Keep namespaces aligned with folders.
- Put shared game code in `src/OsuDroid.Game`.
- Keep platform-specific code in `src/OsuDroid.Android` and `src/OsuDroid.iOS`.

## UI Layout
- Put scene and screen composition in `UI/Views`.
- Put reusable controls and drawables in `UI/Controls`.
- Put route enums and navigation-only types in `UI/Navigation`.
- Do not let a route host turn into a product-UI grab bag.

## Services
- Keep shared interfaces small and focused.
- Put stub or fake implementations under `Services/Stubs`.
- Split platform adapters by concern instead of grouping audio, storage, and URI launching into one file.

## Change Hygiene
- If a file starts collecting unrelated responsibilities, split it before adding more.
- Prefer explicit names and small modules over “helpful” multipurpose containers.
- Match folder moves with namespace updates in the same change so `IDE0130` stays clean.
