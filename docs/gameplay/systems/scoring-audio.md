# Scoring and Audio System

## Score event boundaries

- Gameplay objects report outcomes through `GameObjectListener`; `GameScene` translates those callbacks into score, combo, accuracy, HP, samples, and end-of-object behavior. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObjectListener.java:12-25`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2175-2265`)
- Circle scoring is centralized in `registerHitCircle`, including object result, combo behavior, score event, HP event, accuracy event, and sample playback. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2268-2321`)
- Slider scoring is centralized in `registerHitSlider`, including head/tick/repeat/tail accounting and combo/accuracy result mapping. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2327-2411`)
- Spinner scoring is centralized in `registerHitSpinner`, including clear result, bonus score, HP, accuracy, and combo handling. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2421-2475`)
- Score constants and HP deltas are contract data, not tuning knobs. See `docs/gameplay/contracts/scoring-values.md`.

## Accuracy and HP

- Accuracy registration has a separate path from score registration. Port code must preserve that split so HUD/statistic updates can observe accuracy changes independently. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:3035-3046`)
- HP is updated in object result paths and during break/update logic, not only when a score number appears. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2175-2265`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1611-1643`)
- Accuracy formula is `(hit300 * 6 + hit100 * 2 + hit50) / (6 * notesHit)`, with `1` returned when no notes were hit. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:231-239`)
- HP clamps to `[0, 1]`. Reaching zero consumes life and can fail when `canFail` and life reaches zero. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:128-146`)

## Score models

- ScoreV2 uses max `1,000,000`, combo portion `0.7`, accuracy portion `0.3`, and bonus score added after the base value. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:32-34`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:241-281`)
- ScoreV1 adds raw `amount`; combo-scored events add `int(amount * currentCombo * diffModifier / 25)` when positive. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:263-281`)
- Difficulty score multiplier is initialized from OD, HP, and CS during scene preparation. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1069-1084`)

## Hit samples and metronome

- `GameScene.playHitSounds` receives object sample sets and plays configured sound effects for gameplay hit feedback. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2477-2491`)
- Spinner classes own their continuous spinner samples and stop/restart them according to spinner state. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySpinner.java:188-255`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:137-324`)
- The gameplay loop runs a metronome check against music timing points before object processing finishes. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1831-1845`)

## Song service lifecycle

- Loader entry stops preview playback and transfers ownership to game-scene-controlled playback. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:195-225`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1328-1378`)
- Pause stops or pauses active music/video paths; resume restores gameplay update state and playback. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2651-2705`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2842-2865`)
- End path lowers song volume and routes to result handling after replay-save checks. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1848-1915`)
