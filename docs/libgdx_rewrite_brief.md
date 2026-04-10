# libGDX Android+iOS Rewrite Brief

## Purpose
This file is a compact implementation brief for AI agents working against the existing source tree.

The project direction is **not** an AndEngine port and **not** a KMP migration. It is a **full rewrite** of the game into a new **single shared Java/libGDX codebase** targeting **Android + iOS only**.

## Final decisions
- Use **libGDX** as the runtime/framework.
- Use **Gradle**.
- Use **Java-only shared game code**.
- Use **JDK 17** for the toolchain.
- Keep the project/source compatibility at **Java 8**.
- Targets are **Android** and **iOS** only.
- Do **not** target web.
- Do **not** target desktop as a shipping platform.
- Avoid proprietary data/asset formats where practical.
- Treat this as a **greenfield architecture**, not an adaptation of the old Android app structure.

## What this means
The old project is effectively reference material only.
Its current Android app structure, plugins, AndroidX-heavy wiring, AndEngine-specific code, and old asset layout should **not** be preserved unless needed as a source of gameplay behavior or import compatibility.

## High-level architecture
Create a fresh libGDX project with these modules:

- `core`
  - Entire game logic.
  - Rendering.
  - Screens/flow.
  - Input abstraction.
  - Beatmap/chart parsing.
  - Timing/judgement/scoring.
  - Replay model + serialization.
  - Skin/theme loading.
  - Multiplayer protocol models if needed.
- `android`
  - Thin launcher only.
  - Android-specific integrations only.
- `ios`
  - Thin launcher only.
  - iOS-specific integrations only.
- `tools` or equivalent helper modules/scripts
  - Asset packing.
  - Validation.
  - Importers/converters.
  - Schema checks.

## Architectural rules
1. **No Android APIs in `core`.**
2. `core` must be runnable in principle on any libGDX backend, even if we only ship Android/iOS.
3. Platform modules should stay minimal and mostly delegate into `core`.
4. Audio must be abstracted behind an internal interface from day one.
5. Asset source files should be open/documented formats; packed runtime assets may be generated.
6. Any new custom game data format must be documented and versioned.

## Recommended internal package split
Inside `core`, keep things separate:

- `game.rules`
- `game.timing`
- `game.chart`
- `game.score`
- `game.replay`
- `game.audio`
- `game.assets`
- `game.render`
- `game.screen`
- `game.input`
- `game.skin`
- `game.net` (if multiplayer remains)
- `game.platform` (interfaces only)

## Platform boundary
Define interfaces in `core` for anything platform-specific, for example:

- `AudioBackend`
- `HapticsBackend`
- `FilePickerBackend`
- `ExternalUrlBackend`
- `NotificationBackend`
- `Login/Identity backend` if ever needed

Provide implementations in `android` and `ios`.

## Audio guidance
This is a rhythm game, so audio is a first-class subsystem.
Do **not** tightly couple gameplay timing to a convenience playback API.

Recommended approach:
- Define an internal `AudioBackend` interface.
- Keep song timing, lead-in, offset, and scheduling logic in `core`.
- Allow platform backends to evolve independently if low-latency issues appear.
- Start simple if needed, but do not let the rest of the game depend directly on raw libGDX audio calls everywhere.

## Asset/data policy
### Source-of-truth formats
Prefer open, documented formats:
- Images: `png`, optionally `webp` if acceptable in pipeline
- Music: `ogg`
- Effects/hitsounds: `wav` where useful
- Fonts: `ttf` or `otf`
- Structured data: `json`

### Generated/runtime formats
Allowed as build outputs only:
- Texture atlases
- Packed asset bundles
- Generated bitmap fonts
- Preprocessed metadata caches

### Rule
Keep raw/source assets separate from generated runtime assets.

Suggested layout:
- `assets-raw/`
- `assets-built/`
- `schemas/`
- `tools/`

## Data schemas to define early
Before heavy engine work, define and version these:

- Beatmap/chart schema
- Replay schema
- Skin/theme schema
- Asset manifest schema
- Audio metadata contract
- Save/profile schema
- Multiplayer packet/message schema if multiplayer remains

## What to preserve from the old codebase
Preserve behavior, not structure.

Candidates to preserve conceptually:
- Timing/judgement rules
- Score/combo logic
- Replay semantics
- Beatmap interpretation
- Skin behavior expectations
- Multiplayer rules/protocol semantics
- Import compatibility goals for legacy content

Things **not** worth preserving as architecture:
- AndEngine scene/entity structure
- Android app module layout
- Android resource assumptions
- Android-only storage/preferences code
- Firebase/plugin-driven architecture in shared logic
- Room/KSP-based persistence design inside gameplay code

## Old dependency triage
### Replace
- `AndEngine` -> libGDX runtime + new game architecture

### Re-evaluate carefully
- `LibBASS` -> likely replaced or isolated behind new `AudioBackend`
- Networking libs -> keep only if they remain platform-neutral and actually needed
- Utility libs -> keep only if they do not drag Android assumptions

### Remove from shared game design
- AndroidX UI/material/preferences
- Room / KSP / Android app plugins
- Firebase-specific logic in shared core
- Android-specific permission/UI/resource patterns

## Rendering model
Use standard libGDX 2D building blocks unless a strong reason appears otherwise:
- `Game` / `Screen`
- `SpriteBatch`
- `Texture` / `TextureRegion`
- `OrthographicCamera`
- `AssetManager`
- `scene2d` only where it genuinely helps, not as a forced architecture

## Input model
Build an internal action-oriented input layer.
Do not spread raw touch/key handling everywhere.

Recommended shape:
- platform/libGDX events -> input translator -> game actions/state

This is especially important for rhythm timing, lanes, gestures, and replayable deterministic input handling.

## Determinism and testing
For anything tied to chart playback, judgement, scoring, and replay:
- Favor deterministic pure logic where possible.
- Keep timing/judgement math testable without rendering.
- Add regression tests for replay compatibility and score correctness.

## Migration strategy for agents
### Phase 1: foundation
- Generate fresh libGDX project.
- Establish module boundaries.
- Define schemas.
- Define platform interfaces.
- Define audio abstraction.

### Phase 2: gameplay core
- Implement timing model.
- Implement chart model/parser.
- Implement judgement/scoring.
- Implement replay model.
- Add tests.

### Phase 3: runtime
- Implement screen flow.
- Implement renderer.
- Implement asset pipeline.
- Implement input translation.
- Hook up audio backend.

### Phase 4: platform glue
- Android launcher/integration.
- iOS launcher/integration.
- Storage/import/export details.
- Optional platform services.

### Phase 5: compatibility and polish
- Legacy importers if needed.
- Skin/theme migration tools.
- Performance pass.
- Timing calibration.
- Asset rebuild pipeline.

## Anti-goals
- No KMP-based shared game architecture.
- No editor-centric engine workflow.
- No proprietary opaque asset source format.
- No preservation of Android-only architectural baggage.
- No split rewrite into different gameplay languages for Android vs iOS.

## Default stance for future decisions
When uncertain, prefer:
- simpler shared Java code in `core`
- thinner platform modules
- open/documented file formats
- isolated platform/audio boundaries
- deterministic gameplay logic
- generated runtime assets from open source assets

## One-sentence project summary
Build a new **Java/libGDX rhythm game** with a **single shared `core` codebase** for **Android+iOS**, using **open/documented asset and data formats**, while treating the old Android/AndEngine codebase as a **behavior reference**, not as architecture to preserve.

The new src/ organisation will be `src/moe/osudroid/*` as the game's actual website is called `https://osudroid.moe/`
