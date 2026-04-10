# Asset Strategy

## Goals
- Follow libGDX `AssetManager` conventions for runtime loading.
- Keep raw source assets separate from generated runtime assets.
- Treat user-imported beatmaps, skins, and replays as first-class external content rather than baking them into app assets.

## Layout
- `assets-raw/`: editable source assets in documented formats
- `assets-built/`: generated atlases, manifests, bitmap fonts, and optimized bundles
- `schemas/`: versioned schemas for manifests, skins, replay data, and imported content metadata
- `user-data/`: runtime-imported beatmaps, skins, replays, caches, and save data outside the shipped asset bundle

## Formats
- Images: `png` as the default source format
- Music: `ogg` for shipped music when needed
- Hitsounds/effects: `wav` where latency and editing matter
- Fonts: `ttf` or `otf`, with generated bitmap fonts where runtime requirements demand it
- Structured metadata: `json`

## Runtime loading
- Centralize runtime asset access through `AssetManager`.
- Preload screen-critical bundles during bootstrap and flow transitions.
- Keep gameplay-required textures, fonts, and sound banks in explicit manifests.
- Unload by screen/domain ownership, not ad hoc scattered calls.

## User content
- Imported beatmaps remain loose external content with indexed metadata and generated caches.
- Skins and themes need explicit manifests so missing or partial content resolves predictably.
- Replays remain versioned data files with compatibility guards.

## Build pipeline
- Raw assets are the source of truth.
- Atlases, manifests, font caches, and compatibility caches are generated outputs.
- Generated outputs should be reproducible and version-aware.
- Cache invalidation keys should include source timestamps or content hashes plus schema/tool version.
