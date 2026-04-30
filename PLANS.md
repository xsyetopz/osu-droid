# Gameplay Parity Plan

Status legend:

- `[ ]` Not started.
- `[~]` Started, partial, or blocked by a prerequisite.
- `[x]` Source-parity complete and validated.

End state: every checkbox in this file is `[x]`. Gameplay then matches original osu!droid behavior, values, routing, scoring, audio, UI, assets, replay modes, and multiplayer hooks as specified by `docs/gameplay/`.

## Reference gates

- [x] Gameplay source study exists under `docs/gameplay/` with cited original source paths.
- [x] Numeric contracts exist for timing, geometry, UI animation, scoring, HP, ranks, and skip. See `docs/gameplay/contracts/timing-geometry-values.md`, `docs/gameplay/contracts/scoring-values.md`, and `docs/gameplay/contracts/ui-animation-values.md`.
- [x] Porting order exists. See `docs/gameplay/implementation/porting-sequence.md`.
- [x] Validation checklist exists. See `docs/gameplay/implementation/validation-checklist.md`.
- [ ] Every gameplay implementation change cites the matching `docs/gameplay/` section in commit notes, PR notes, or local review notes.
- [ ] Any source/screenshot disagreement is recorded as source behavior versus screenshot evidence before code changes continue.

## Route wiring

- [~] Song Select can select beatmap set and difficulty; start-game action still must enter gameplay loader. See `docs/gameplay/flows/song-select-to-gameplay.md`.
- [~] Mod Select exists and owns selected mod state; selected mods still must be handed to gameplay start as authoritative runtime input. See `docs/gameplay/systems/mods.md`.
- [ ] Add Song Select start action that stops preview handoff only through gameplay loader flow.
- [ ] Route selected beatmap, selected difficulty, per-beatmap options, active mods, replay context, and multiplayer context into gameplay request.
- [ ] Route loader cancel back to Song Select with selected preview restart semantics.
- [ ] Route gameplay end to result scene with replay-save eligibility and reduced song volume.
- [ ] Route gameplay Back to pause overlay through hold-to-pause only.
- [ ] Route pause Continue, Retry, and Back to Menu exactly as source behavior.

## Gameplay state contracts

- [ ] Add gameplay request id. Every load, storyboard, video, beatmap parse, and difficulty job must ignore stale request ids. See `docs/gameplay/systems/game-scene.md`.
- [ ] Add gameplay session model: beatmap identity, parsed hit objects, selected mods, replay mode, autoplay, autopilot, multiplayer flags, offsets, and rate.
- [ ] Add scene state: not-loaded, loading, ready-to-start, running, paused, failed, ending, exited.
- [ ] Add four gameplay layers: background, midground, foreground, HUD. See `docs/gameplay/systems/game-scene.md`.
- [ ] Add object pools or equivalent lifetime ownership for circles, sliders, spinners, effects, cursors, and HUD events.
- [ ] Add beatmap cache behavior matching source request lifecycle and invalidation points.
- [ ] Add cancellation path for all load-owned work.

## Loader entry

- [ ] Implement loader background: beatmap `::background`, safe background fallback, crop scale, black dim `alpha=0.7`.
- [ ] Implement loader metadata panel: title width `700`, autoscroll speed `30`, difficulty, artist, selected mods, epilepsy warning, progress spinner `32x32`.
- [ ] Implement loader Back button outside multiplayer only, icon `28x28`, click/cancel sounds, solo cancel behavior.
- [ ] Implement loader quick settings: per-beatmap offset `-250..250ms`, step buttons `-5/-1/+1/+5`, database persistence.
- [ ] Implement loader settings: background brightness `0..100%` default `25%`, storyboard checkbox, video checkbox, scoreboard checkbox.
- [ ] Implement brightness behavior: dim alpha `1 - brightness / 100`, storyboard/video reload on brightness change.
- [ ] Implement restart quick-setting touch behavior: `fadeTimeout=1500ms`, `minimumTimeout=1500ms`, idle alpha decay `delta * 1.5` down toward `0.5`.
- [ ] Implement ready transition: main panel `fadeOut(0.1 OutExpo)`, dim `fadeTo(1 - bgbrightness / 100, 0.2)`, HUD scale `0.9 -> 1` over `0.2 OutCubic`, HUD `fadeIn(0.1 OutExpo)`.
- [ ] Start gameplay only after loader ready state and minimum timeout rules pass.

## Runtime loop

- [ ] Implement gameplay clock with elapsed time, total offset, rate-adjusted offset, frame-offset fix, and per-beatmap offset.
- [ ] Implement music start: lead-in, first-object preempt start time, offset handling, BGM volume, preview stop handoff.
- [ ] Implement video/storyboard jobs with brightness gating and request-id cancellation.
- [ ] Implement timing point selection, beat length, current beat time, samples-match-playback-rate, and kiai state updates.
- [ ] Implement object spawn window from object preempt.
- [ ] Implement active-object update order before HUD/result routing.
- [ ] Implement passive update path for loader and skip seek without scoring advancement.
- [ ] Implement fail state and end routing when HP/life reaches failure condition or beatmap ends.
- [ ] Implement result handoff with replay-save eligibility, score submission flags, and difficulty calculation resume behavior.

## Timing and geometry

- [ ] Implement hit windows: Droid `75/150/250` formulas and Precise `55/120/180` formulas from `docs/gameplay/contracts/timing-geometry-values.md`.
- [ ] Implement fixed miss window `400ms`.
- [ ] Implement preempt mapping: AR0 `1800ms`, AR5 `1200ms`, AR10 `450ms`.
- [ ] Implement fade-in formula `400 * min(1, preempt / 450)`.
- [ ] Implement playfield coordinate system: map `512x384`, actual height `RES_HEIGHT * 0.8`, actual width `height / 3 * 4`.
- [ ] Implement object radius: `64 * gameplayScale * RES_HEIGHT / 480`.
- [ ] Implement droid stack offset multiplier `-4f`.
- [ ] Implement screen-coordinate conversion from osu!pixels to actual playfield coordinates.

## Cursor and input modes

- [ ] Implement cursor event model with down/move/up, hit time, system time, latest non-up event, and closest event before timestamp.
- [ ] Implement start-time ordered notelock by consuming each cursor down event once.
- [ ] Implement normal hit-test: event time within hittable range and squared distance within screen radius.
- [ ] Implement autopilot hittable range: `clamp(mehWindow + 100, 200, 400) / 1000`.
- [ ] Implement replay input source without normal touch mutation.
- [ ] Implement autoplay cursor generation and HUD touch-down events.
- [ ] Implement autopilot cursor behavior separately from autoplay scoring.
- [ ] Implement show-cursor setting default hidden unless replay or explicit show cursor.

## Hit circles

- [ ] Implement circle sprites: hit circle, overlay, approach circle, number sprites, score/result effect.
- [ ] Implement circle lifecycle: hidden before visible start, fade in, approach scale, hit, miss, finish, pool return.
- [ ] Implement circle judgement: miss when `accuracy > mehWindow / 1000`, 300 when `<= greatWindow / 1000`, 100 when `<= okWindow / 1000`, else 50.
- [ ] Implement forced replay result override for miss/300/100/50.
- [ ] Implement hit samples, burst effect, hit result effect, vibration, HUD note-hit event.
- [ ] Implement combo-end flags and combo color handling.
- [ ] Validate circle timing against original source at OD 0, 5, 10, and Precise mod.

## Sliders

- [ ] Implement slider path conversion and rendering thresholds: `32` osu!px for linear path, `6` osu!px otherwise when snaking-out is off.
- [ ] Implement slider body, ball, reverse arrows, ticks, follow circle, endpoint state, repeat counters, and tail state.
- [ ] Implement slider tick generation from path distance and repeat direction.
- [ ] Implement follow circle visibility only while pressed inside follow radius.
- [ ] Implement slider start score `30`, repeat score `30`, tick score `10`, and tail final result.
- [ ] Implement nested miss score `-1`, combobreak sound when combo `> 30`, SuddenDeath HP failure path.
- [ ] Implement whole-slider miss score `0`.
- [ ] Implement slider head/tick/repeat/end statistic counters.
- [ ] Implement snaking-in and snaking-out settings.
- [ ] Validate slider tick, repeat, tail, and miss behavior with fixture beatmaps.

## Spinners

- [ ] Implement classic spinner visuals: center, top/bottom overlays, clear/approach sprites, RPM counters, looping samples.
- [ ] Implement modern spinner visuals and source-selected spinner style.
- [ ] Implement rotation math from cursor angle delta around screen center.
- [ ] Implement `needRotations`, `fullRotations`, rotation accumulator, clear state, and RPM display.
- [ ] Implement spinner grading: 50 when filled percent `> 0.9`, 100 when `> 0.95`, 300 when clear, replay override by `accuracy % 4`.
- [ ] Implement spinner bonus score `1000` after clear.
- [ ] Implement spinner HP recovery formulas tied to health drain and duration.
- [ ] Implement spinner samples: spin loop, bonus sample, stop on object finish.
- [ ] Validate classic and modern spinner behavior against source fixtures.

## Scoring, HP, rank, and replay result data

- [ ] Implement `StatisticV2` equivalent with score, combo, max combo, HP, life, alive state, hit counts, slider counts, bonus score, score hash, and forced score.
- [ ] Implement HP deltas for 300, 100, 50, miss, slider nested values, and spinner bonus from `docs/gameplay/contracts/scoring-values.md`.
- [ ] Implement ScoreV1 formula: raw amount plus combo bonus `int(amount * currentCombo * diffModifier / 25)`.
- [ ] Implement ScoreV2 formula: max `1_000_000`, combo portion `0.7`, accuracy portion `0.3`, accuracy exponent `10`, plus bonus score.
- [ ] Implement accuracy formula `(hit300 * 6 + hit100 * 2 + hit50) / (6 * notesHit)`, with no-hit accuracy `1`.
- [ ] Implement ranks `XH`, `X`, `SH`, `S`, `A`, `B`, `C`, `D` with exact thresholds.
- [ ] Implement replay object result encoding for circles and spinners.
- [ ] Implement score invalidation and score hash behavior where source requires it.
- [ ] Validate score/accuracy/HP/rank with deterministic object-result fixtures.

## Audio

- [ ] Implement object hit sample resolution from sample sets and beatmap sample control points.
- [ ] Implement custom skin/beatmap sound preference interaction.
- [ ] Implement circle, slider, spinner, combobreak, menuhit, click-short, click-short-confirm samples.
- [ ] Implement metronome sample behavior tied to timing points.
- [ ] Implement song service pause, resume, seek, stop, volume, and rate behavior.
- [ ] Implement samples-match-playback-rate and rate-mod pitch behavior.
- [ ] Validate sample timing and stop/resume behavior during pause, skip, fail, retry, and end.

## HUD

- [ ] Implement default HUD elements: accuracy, leaderboard, combo, pie progress, health bar, score, back button, unstable rate, average offset, hit error meter.
- [ ] Implement default layout adjustments: accuracy below score and pie progress beside accuracy.
- [ ] Implement scoreboard visibility: remove leaderboard when `showscoreboard=false` outside HUD editor.
- [ ] Implement HUD event fanout: update, touch down, object lifetime start, note hit, break state, accuracy register.
- [ ] Implement HP/progress, score, accuracy, leaderboard, combo, offset, unstable rate, FPS, and hit error visuals.
- [ ] Implement HUD editor-safe behavior if HUD editor exists before gameplay completion.
- [ ] Validate default screenshot baseline against `docs/gameplay/contracts/settings-baseline.md`.

## Hold-to-pause and pause menu

- [ ] Implement HUD back button size `72`, arrow texture `back-arrow`, front circle color `0xFF002626`, front circle size `0.95`.
- [ ] Implement hold duration default `300ms` from `back_button_press_time`.
- [ ] Implement hold progress: fill portion `progress`, scale `1 + progress / 2`, alpha interpolation `0.25 -> 0.5 OutCubic`.
- [ ] Implement release decay at `2x` real elapsed time.
- [ ] Implement move-outside cancel for local touch outside bounds.
- [ ] Implement pause: stop gameplay update progression, pause music/video, show dim overlay and PauseMenu.
- [ ] Implement pause menu buttons: Continue, Retry, Back to Menu.
- [ ] Implement Continue resume behavior for scene updates, music, and video.
- [ ] Implement Retry quit/restart behavior through gameplay exit path.
- [ ] Implement Back to Menu replay-save condition and route behavior.
- [ ] Validate hold-to-pause with short tap, partial hold, release decay, move-outside cancel, full hold, resume, retry, menu exit.

## Breaks, countdown, and skip

- [ ] Implement break window detection from beatmap break periods and current song position.
- [ ] Implement BreakAnimator visuals: ranking mark, approach arrows, break show/hide/reset.
- [ ] Implement HP recovery during breaks.
- [ ] Implement break dim behavior with `noChangeDimInBreaks=false` default.
- [ ] Implement countdown sprites and timing by music position.
- [ ] Implement `Config.isCorovans()` countdown conditional behavior.
- [ ] Implement initial skip target: `firstObjectStartTime - max(2s, firstObjectTimePreempt)`, `skipTime = skipTargetTime - 1`.
- [ ] Implement skip touch area within `250px` radius from bottom-right corner.
- [ ] Implement solo skip seek with music/video clamped seek times.
- [ ] Implement multiplayer skip request path through room API equivalent.
- [ ] Validate break/countdown/skip with intro-only, long-break, no-break, and multiplayer-room scenarios.

## Mods integration

- [~] Mod Select UI and state exist; gameplay consumption must be added.
- [ ] Pass selected mods to gameplay session and difficulty/runtime systems.
- [ ] Apply mod score multiplier before gameplay start.
- [ ] Apply difficulty-changing mods to AR, OD, CS, HP, preempt, hit windows, BPM, and star preview consistently with ModMenu.
- [ ] Apply rate mods to music, video, storyboard, hit samples, and elapsed-time math.
- [ ] Apply Relax, Autopilot, Autoplay, Hidden, Flashlight, Traceable, Easy, HardRock, SuddenDeath, Perfect, Muted, FreezeFrame, ApproachDifferent, ScoreV2, and Precise behavior where source defines gameplay effects.
- [ ] Preserve multiplayer mod ownership and free-mod restrictions.
- [ ] Preserve DoubleTime/NightCore multiplayer preference conversion.
- [ ] Validate mod combinations against source-visible ModMenu rules.

## Assets and skin behavior

- [ ] Audit all gameplay texture keys from `docs/gameplay/contracts/asset-keys.md` against runtime owned assets.
- [ ] Add missing gameplay textures, sounds, and skin fallbacks under owned runtime assets, not runtime reads from `third_party/osu-droid-legacy`.
- [ ] Implement high-quality asset loading needs for loader and HUD back arrow.
- [ ] Implement beatmap background crop and safe-background fallback.
- [ ] Implement combo colors from beatmap, custom config, and skin force override.
- [ ] Implement hit lighting, comboburst, first approach circle, dim hit objects, and custom combo-color settings.
- [ ] Validate default asset and skin fallback behavior with no custom skin, custom skin, missing texture, and beatmap-sound scenarios.

## Replay, autoplay, autopilot, and multiplayer

- [ ] Implement replay load path and replay version handling.
- [ ] Implement replay cursor stream and object result stream.
- [ ] Implement replay score submission restrictions and replay-save eligibility.
- [ ] Implement autoplay object interaction for circles, sliders, and spinners.
- [ ] Implement autopilot movement and hit-test differences without autoplay scoring shortcuts.
- [ ] Implement multiplayer score sync chunks and last-score-sent behavior.
- [ ] Implement multiplayer fail, skip request, room mod, and submit-score settings.
- [ ] Validate replay playback determinism with recorded source-compatible replay data.

## Settings baseline

- [ ] Default storyboard off.
- [ ] Default video off.
- [ ] Default background brightness `25%`.
- [ ] Default cursor hidden unless enabled or replay path shows it.
- [ ] Default scoreboard shown.
- [ ] Default frame-offset fix on.
- [ ] Default dim hit objects on.
- [ ] Default break dim change enabled.
- [ ] Loader quick settings update active runtime values immediately where source does.
- [ ] Validate baseline screenshot setup before visual comparisons.

## Validation gates

- [ ] Add unit fixtures for hit windows, preempt, screen coordinate conversion, score formulas, HP deltas, ranks, and skip seek math.
- [ ] Add object-level simulation tests for circles, sliders, spinners, breaks, pause, skip, and end routing.
- [ ] Add audio lifecycle tests or trace assertions for start, pause, resume, skip, retry, fail, and result.
- [ ] Add replay/autoplay/autopilot deterministic tests.
- [ ] Add source-reference fixture beatmaps covering normal, dense, slider-heavy, spinner-heavy, long intro, breaks, rate mods, and failure.
- [ ] Build Android and iOS targets after each gameplay milestone.
- [ ] Install and launch on connected iPhone after visual gameplay UI edits.
- [ ] Capture screenshots for loader, first object, HUD, pause menu, break, countdown, skip, spinner, and result route.
- [ ] Compare screenshots against accepted osu!droid reference evidence or record source-versus-screenshot divergence.
- [ ] Run `python3 scripts/dev/check-stale-terms.py` after docs/tracker edits.
- [ ] Run architecture audit before adding large gameplay subsystems.

## Parity completion gate

- [ ] Song Select to loader to gameplay to pause/result/menu loop works on Android and iOS.
- [ ] Gameplay values match every table in `docs/gameplay/contracts/`.
- [ ] All hit object families match source lifecycle, scoring, audio, visual, and replay behavior.
- [ ] Runtime loop matches source timing, offsets, object update order, break/countdown/skip, failure, and end routing.
- [ ] HUD and pause match source default layout, values, animation, and interactions.
- [ ] ModMenu wiring and gameplay mod effects match source behavior, including multiplayer restrictions.
- [ ] Replay, autoplay, autopilot, and multiplayer hooks match source behavior.
- [ ] Assets and skin fallbacks match source-owned runtime behavior.
- [ ] Validation fixtures, screenshots, device builds, and source citations prove parity.
- [ ] All checklist items above are `[x]`.
