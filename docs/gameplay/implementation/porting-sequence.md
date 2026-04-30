# Porting Sequence

Use this order to avoid wiring UI before authoritative state exists.

## 1. State contracts

- Define gameplay request id, beatmap identity, active mods, replay context, multiplayer context, and load cancellation state first. Source request-id and cancellation behavior: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:939-1056`.
- Define four scene layers before object rendering: background, midground, foreground, HUD. Source layer ownership: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:138-325`.

## 2. Loader

- Port loader visuals and settings controls before music start. Source loader UI: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:38-165`.
- Port cancel/back path before successful start path. Source cancel: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:168-187`.
- Port transition into gameplay only after loader ready state exists. Source start transition: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:195-225`.

## 3. Runtime loop

- Port timing and object spawning before score visuals. Source update loop: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1394-1724`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1729-1845`.
- Port pause/resume/end paths before adding replay save UI. Source pause/end: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1848-1915`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2651-2705`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2842-2865`.

## 4. Object families

- Circles first: they validate input timing, score callbacks, samples, and HUD events with the smallest object state. Source: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayHitCircle.java:221-297`.
- Sliders second: they add path position, ticks, repeats, follow circle, and tail scoring. Source: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySlider.java:803-1288`.
- Spinners third: they add continuous cursor rotation and continuous samples. Source: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySpinner.java:131-255`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:137-324`.

## 5. HUD, breaks, and skip

- Port default HUD element list from `HUDSkinData`, then event fanout from `GameplayHUD`. Sources: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/HUDSkinData.kt:27-96`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/GameplayHUD.kt:261-305`.
- Port break animator and countdown after timing state is stable. Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:39-177`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/Countdown.java:24-167`.
- Port skip after next-object timing and multiplayer context exist. Source: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1941-2064`.

## 6. ModMenu integration

- ModMenu should hand selected mods to gameplay; gameplay should not own mod selection UI. Source: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:42-68`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:1095-1114`.
- Preserve multiplayer mod ownership rules before using room state in gameplay. Source: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:454-539`.

