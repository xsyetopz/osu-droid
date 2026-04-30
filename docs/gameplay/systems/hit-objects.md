# Hit Object System

## Shared object contract

- Every gameplay object owns `startTime`, `endTime`, `comboNumber`, `comboIndex`, `hit`, `finished`, position, color, and listener callbacks. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObject.java:15-69`)
- Object lifecycle is pull-updated by `GameScene`: objects enter the active list before their visual start, receive cursor state while active, and are removed when finished. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1729-1828`)
- `GameObjectListener` is the scoring bridge. Objects do not mutate the score model directly; they call circle, slider, spinner, accuracy, HP, combo, sample, and object-end callbacks. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObjectListener.java:12-25`)
- Normal hit detection consumes cursor down events in order. This preserves notelock: each cursor down event is processed once; other cursors are checked at the closest event before the down event. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObject.java:137-177`)
- Distance check uses squared distance from object screen position to cursor position against squared screen-space gameplay radius. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObject.java:199-213`)

## Timing and geometry values

| Contract | Value |
| --- | --- |
| Miss window | `400ms` fixed. |
| Droid hit windows | `300 = 75 + 5 * (5 - OD)`, `100 = 150 + 10 * (5 - OD)`, `50 = 250 + 10 * (5 - OD)`. |
| Precise hit windows | `300 = 55 + 6 * (5 - OD)`, `100 = 120 + 8 * (5 - OD)`, `50 = 180 + 10 * (5 - OD)`. |
| Approach preempt | AR maps through `1800ms` at AR0, `1200ms` at AR5, `450ms` at AR10. |
| Circle radius | `64 * gameplayScale * RES_HEIGHT / 480` in screen pixels. |

Full citations and formulas: `docs/gameplay/contracts/timing-geometry-values.md`.

## Hit circles

- A circle creates its circle sprite, approach circle, score/result sprite, number sprites, and follow-up state from the object data. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayHitCircle.java:51-169`)
- During update, the approach circle scales toward the hit circle, the circle fades in before hit time, and a missed circle is judged after the miss window. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayHitCircle.java:221-297`)
- Touch processing checks cursor distance and hit timing. Valid hits call `registerHitCircle`, play object samples, emit combo/accuracy events, and finish the object. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayHitCircle.java:221-297`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2268-2321`)
- Circle result thresholds use absolute timing error in seconds: miss if `accuracy > mehWindow / 1000`, 300 if `accuracy <= greatWindow / 1000`, 100 if `accuracy <= okWindow / 1000`, else 50. Forced replay result overrides thresholds. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2300-2315`)

## Sliders

- Slider initialization builds path sprites, reverse arrows, ticks, follow circle, endpoint state, repeat counters, and slider timing. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySlider.java:300-461`)
- Slider ticks are generated from tick distance and path position. The Kotlin helper emits timestamped tick positions, including repeat-direction mapping. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/game/SliderTicks.kt:79-79`)
- Runtime update advances along the curve, shows the follow circle only while pressed in range, tracks tick hits, repeat hits, tail state, and calls slider scoring callbacks. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySlider.java:803-908`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySlider.java:911-1288`)
- Slider scoring separates head, ticks, repeats, tail, combo, and final object grade. `GameScene.registerHitSlider` converts object callbacks into combo, score, HP, and accuracy effects. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySlider.java:1315-1360`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2327-2411`)
- Slider nested score values are concrete: head `30`, repeat `30`, tick `10`; tail uses final slider result. Nested miss uses `score=-1` and can trigger combobreak and SuddenDeath. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2350-2405`)

## Spinners

- Classic spinner initializes center sprites, top/bottom circle overlays, clear/approach sprites, RPM counters, and sound state. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySpinner.java:30-128`)
- Classic spinner update integrates cursor angle deltas into rotation amount, tracks rotations per minute, triggers clear state, plays spinner samples, and awards bonus score after clear. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySpinner.java:131-186`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySpinner.java:188-255`)
- Modern spinner has separate visual state and rotation math, but still reports spinner result through the same listener callback path. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:24-135`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:137-324`)
- `GameScene.registerHitSpinner` applies spinner result, bonus score, HP, combo, accuracy, and sample effects. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2421-2475`)
- Modern spinner grades by filled percent: `> 0.9` gives 50, `> 0.95` gives 100, clear gives 300. Replay result can override by `accuracy % 4`. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:301-323`)
- Spinner bonus emits `score=1000`, which adds bonus score without direct combo/HP mutation in `StatisticV2.registerHit`. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2421-2425`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:159-177`)

## Cursor modes

- Normal play passes touch events from `GameScene.onAreaTouched` to active objects and updates object cursor lists each frame. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2521-2618`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1394-1508`)
- Replay, autoplay, and autopilot route through the update loop before object processing. Gameplay input must keep those modes as data sources, not separate object implementations. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1394-1508`)
