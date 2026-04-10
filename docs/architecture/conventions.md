# Conventions

## Design rules
- Prefer small cohesive modules with one reason to change.
- Preserve domain knowledge once. Do not duplicate timing formulas, score rules, beatmap interpretation, or replay semantics across systems.
- Keep data-heavy hot paths simple and explicit. Avoid reflection-heavy dependency injection and hidden service locators.

## Package rules
- `game.chart`: beatmap parsing, normalized chart models, timing/control-point interpretation
- `game.timing`: clocks, offsets, playback coordination, lead-in, and deterministic step timing
- `game.rules`: hit processing, object lifecycles, judgement windows, combo, fail logic
- `game.score`: score state, ranking, accuracy, and result calculation
- `game.replay`: replay model, encoding/decoding, playback, validation
- `game.input`: action translation, lane/finger state, gesture normalization
- `game.render`: gameplay draw models, HUD presentation state, skin-driven draw decisions
- `game.screen`: libGDX screens and flow coordinators
- `game.assets`: manifests, loaders, skin/theme metadata, and asset contracts
- `game.net`: multiplayer contracts and match/session models
- `game.platform`: platform-facing interfaces only

## Naming
- Types are nouns: `BeatmapChart`, `JudgementWindow`, `ReplayFrame`, `GameplayClock`
- Functions are verbs: `parseBeatmap`, `advanceClock`, `resolveHit`, `serializeReplay`
- Avoid generic names such as `Manager`, `Helper`, `Util`, `Process`, and `Handle`

## Allowed patterns
- Constructor injection for explicit dependencies
- Immutable configuration objects at subsystem boundaries
- Clear command/event objects for cross-system handoffs
- Narrow interfaces at platform boundaries

## Forbidden patterns
- Android or iOS APIs in `core`
- Static global mutable state for gameplay/session ownership
- Carrying forward AndEngine scene/entity structure by habit
- Screen classes that also parse beatmaps, score hits, manage sockets, and own persistence
- Catch-all service classes that mix gameplay, UI, storage, and networking

## Testing defaults
- Every preserved gameplay rule needs unit-level parity coverage.
- Replay semantics need regression tests.
- Beatmap parsing and timing need fixture-based tests.
- Rendering logic can be thinner and more integration-oriented, but simulation rules must stay testable without graphics.
