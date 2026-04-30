# Timing and Geometry Values

These values are implementation inputs. Keep units unchanged unless the port layer explicitly converts them.

## Hit windows

| Value | Formula / constant | Unit | Source |
| --- | --- | --- | --- |
| Miss window | `400.0` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/HitWindow.kt:31-36` |
| Droid 300 window | `75 + 5 * (5 - OD)` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/DroidHitWindow.kt:18-28` |
| Droid 100 window | `150 + 10 * (5 - OD)` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/DroidHitWindow.kt:18-28` |
| Droid 50 window | `250 + 10 * (5 - OD)` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/DroidHitWindow.kt:18-28` |
| Precise 300 window | `55 + 6 * (5 - OD)` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/PreciseDroidHitWindow.kt:18-28` |
| Precise 100 window | `120 + 8 * (5 - OD)` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/PreciseDroidHitWindow.kt:18-28` |
| Precise 50 window | `180 + 10 * (5 - OD)` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/PreciseDroidHitWindow.kt:18-28` |
| Autopilot hittable range | `clamp(mehWindow + 100, 200, 400) / 1000` | sec | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObject.java:187-197` |
| Normal hittable range | `400 / 1000` | sec | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObject.java:187-197` |

## Approach timing

| Value | Formula / constant | Unit | Source |
| --- | --- | --- | --- |
| AR 0 preempt | `1800.0` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:487-503` |
| AR 5 preempt | `1200.0` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:487-503` |
| AR 10 preempt | `450.0` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:487-503` |
| Object preempt | `difficultyRangeInt(AR, 1800, 1200, 450)` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:357-384` |
| Fade-in time | `400 * min(1.0, timePreempt / 450)` | ms | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:357-384` |
| Stored original preempt | `difficultyRange(AR, 1800, 1200, 450)` | ms | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:691-715` |

## Playfield geometry

| Value | Formula / constant | Unit | Source |
| --- | --- | --- | --- |
| Beatmap coordinate width | `512` | osu!px | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Constants.java:3-10` |
| Beatmap coordinate height | `384` | osu!px | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Constants.java:3-10` |
| Actual playfield height | `RES_HEIGHT * 0.8` | screen px | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Constants.java:3-10` |
| Actual playfield width | `MAP_ACTUAL_HEIGHT / 3 * 4` | screen px | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Constants.java:3-10` |
| Circle base radius | `64f` | osu!px | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:487-503` |
| Screen scale | `gameplayScale * RES_HEIGHT / 480` | scale | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:216-235` |
| Screen radius | `64 * screenSpaceGameplayScale` | screen px | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:216-235` |
| Droid stack offset multiplier | `-4f` | osu!px scale | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:357-384` |
| Standard stack offset multiplier | `-6.4f` | osu!px scale | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:357-384` |

## Screen coordinate conversion

```text
xScale = MAP_ACTUAL_WIDTH / 512
yScale = MAP_ACTUAL_HEIGHT / 384
xOffset = (RES_WIDTH - MAP_ACTUAL_WIDTH) / 2
yOffset = (RES_HEIGHT - MAP_ACTUAL_HEIGHT) / 2
screenX = osuX * xScale + xOffset
screenY = osuY * yScale + yOffset
```

Source: `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/hitobject/HitObject.kt:429-442`

