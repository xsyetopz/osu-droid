# Break, Countdown, and Skip System

## Break state

- `GameScene` derives break windows from beatmap break periods and the current song position. During a break it updates break visuals, HP recovery, object visibility, and HUD state. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1611-1643`)
- `BreakAnimator` owns break visual sprites, ranking mark display, approach arrows, and timing-dependent visibility. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:39-61`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:77-108`)
- Break visuals are shown by `showBreak`, hidden by `hideBreak`, and reset through explicit animator calls rather than implicit scene removal. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:110-120`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:127-177`)

## Countdown

- `Countdown` creates countdown sprites and updates them by current music position, not by wall-clock frame count. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/Countdown.java:24-109`)
- Countdown display uses timing thresholds and includes `Config.isCorovans()` conditional behavior. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/Countdown.java:117-167`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:335-340`)

## Skip button

- Skip button availability is computed in the gameplay update path from next-object timing, replay state, multiplayer state, and intro length. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1941-1970`)
- Skip action seeks to a calculated target before the next relevant object and has a multiplayer request path when local immediate skip is not allowed. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2018-2064`)
- Initial skip target is `firstObjectStartTime - max(2s, firstObjectTimePreempt)`, with `skipTime = skipTargetTime - 1`. Beatmap audio lead-in can move initial elapsed time earlier. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:746-755`)
- Skip button touch area is distance `< 250 * 250` from bottom-right screen corner `(RES_WIDTH, RES_HEIGHT)`. Multiplayer sends `RoomAPI.requestSkip`; solo calls `skip()`. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1941-1970`)
- Skip no-ops when `elapsedTime > skipTime - 1` unless forced. Seek time uses `ceil(elapsedTime * 1000)`, subtracts `totalOffset * rate * 1000` for music, subtracts `videoOffset * 1000` for video, and clamps both to `>= 0`. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2018-2064`)

## Background dim rules

- Loading dim is a fixed overlay value inside `GameLoaderScene`; gameplay background brightness comes from `Config.getBGBrightness()`. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:38-165`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:775-780`)
- Break state does not replace the beatmap background owner. Break visuals and ranking marks are overlays above the gameplay background. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:39-61`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1611-1643`)
