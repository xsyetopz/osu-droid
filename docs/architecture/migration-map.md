# Migration Map

## Preserve
- Beatmap parsing semantics
- Timing and offset behavior
- Judgement windows and hit resolution rules
- Score, combo, and accuracy behavior
- Replay encoding, playback, and validation semantics
- Skin/theme expectations that matter to gameplay readability
- Multiplayer room and match semantics if multiplayer stays in scope

## Discard
- AndEngine scene ownership and rendering structure
- Android `Activity`-centric gameplay architecture
- Shared mutable globals that own runtime state
- Firebase-, Room-, and Android-resource-driven assumptions in shared gameplay code
- The current mixed Java/Kotlin package layout

## First rewrite systems
1. libGDX project spike with `core`, `android`, `ios`, `lwjgl3`, and `tools`
2. Beatmap parser and normalized chart model
3. Gameplay clock, timing, and deterministic update loop
4. Judgement and scoring subsystems
5. Replay model and playback
6. Asset manifests and skin/theme loader contracts
7. Menu and gameplay screen flow

## Legacy hotspots worth tracing
- `src/ru/nsu/ccfit/zuev/osu/MainActivity.java`
- `src/ru/nsu/ccfit/zuev/osu/game/GameScene.java`
- `src/ru/nsu/ccfit/zuev/osu/MainScene.java`
- `src/com/osudroid/ui/v2/GameLoaderScene.kt`
- `src/com/osudroid/multiplayer/api/RoomAPI.kt`
- `src/com/osudroid/data/Database.kt`
- `src/com/rian/osu/difficulty/calculator/DroidDifficultyCalculator.kt`
- `src/com/rian/osu/replay/ThreeFingerChecker.kt`

## Parity tests required before broad feature work
- Beatmap parser fixtures for representative maps and edge cases
- Timing/judgement fixtures covering difficulty modifiers and replay playback
- Score/combo/accuracy regression fixtures
- Replay round-trip and compatibility tests
- Screen-flow tests for load, play, results, and retry loops
- Multiplayer model/protocol tests if the rewrite keeps online play
