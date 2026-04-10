# Runtime Model

## Goal
Build the rewrite as a fresh libGDX application with thin platform launchers and one shared gameplay `core`.

## Module shape
- `core`: gameplay rules, timing, beatmap parsing, scoring, replay, input abstraction, render model, skin/theme interpretation, asset contracts, and platform interfaces
- `android`: a single launcher activity plus Android-only adapters for storage, haptics, permissions, and platform services
- `ios`: a thin launcher plus iOS-only adapters for storage, haptics, and platform services
- `lwjgl3`: desktop development backend for fast iteration, input debugging, replay validation, and asset sanity checks
- `tools`: asset packing, schema validation, importers, cache builders, and compatibility utilities

## Flow
- The application root extends libGDX `Game`.
- Each major flow is a `Screen`: bootstrap, main menu, song select, gameplay loader, gameplay, results, multiplayer lobby, and settings.
- `Screen` classes own presentation flow and long-lived subsystem wiring, but they do not own gameplay rules.
- Gameplay simulation lives in deterministic subsystems under `core`. Rendering consumes simulation state instead of mutating it directly.

## Core boundaries
- `core` cannot import Android, iOS, Firebase, Room, AndEngine, or platform UI APIs.
- Beatmap parsing, timing, judgement, score, replay, and gameplay state transitions must be testable without rendering.
- Platform services are injected through explicit interfaces such as `AudioBackend`, `StorageBackend`, `HapticsBackend`, `ExternalUiBackend`, `AuthBackend`, and `NetworkStatusBackend`.

## State model
- Keep gameplay state explicit and serializable where practical.
- Use deterministic update steps for timing, hit resolution, scoring, and replay playback.
- Translate raw touch/device events into domain input commands before they reach gameplay logic.
- Keep menus and overlays separate from gameplay simulation so they can evolve without disturbing timing code.

## Rendering
- Use `SpriteBatch`, viewports, cameras, texture atlases, and custom draw paths for gameplay and HUD rendering.
- Use `scene2d.ui` for non-gameplay UI such as settings, dialogs, login/account flows, and tool-like screens.
- Avoid rebuilding the legacy scene-graph shape inside libGDX. Preserve behavior, not scene ownership structure.

## Multiplayer
- Multiplayer protocol, room models, and match state semantics belong in `core` contracts.
- Socket/session adapters belong outside `core`.
- Multiplayer UI is a screen-level concern, not a shared singleton concern.
