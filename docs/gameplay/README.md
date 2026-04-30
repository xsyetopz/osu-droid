# Gameplay Reference Pack

This directory maps original osu!droid gameplay behavior for the .NET rewrite. Citations use exact source paths and line ranges.

## Read by task

| Task                   | Read                                                                                                                                        |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Loader and start       | `contracts/reference-rules.md`, `contracts/settings-baseline.md`, `flows/loading-entry.md`, `systems/game-scene.md`, `procedures/loader.md` |
| Runtime loop           | `contracts/timing-geometry-values.md`, `systems/game-scene.md`, `procedures/runtime-loop.md`, `implementation/porting-sequence.md`           |
| HUD and pause          | `contracts/ui-animation-values.md`, `systems/hud.md`, `flows/pause-resume-exit.md`, `procedures/pause-exit.md`, `contracts/asset-keys.md`   |
| Hit objects            | `contracts/timing-geometry-values.md`, `contracts/scoring-values.md`, `systems/hit-objects.md`, `systems/scoring-audio.md`, `procedures/hit-objects.md`, `procedures/scoring.md` |
| Break, countdown, skip | `contracts/timing-geometry-values.md`, `systems/breaks-countdown-skip.md`, `procedures/runtime-loop.md`, `contracts/asset-keys.md`           |
| Mods handoff           | `systems/mods.md`, `flows/song-select-to-gameplay.md`, `procedures/mods.md`                                                                 |
| Validation             | `implementation/validation-checklist.md`                                                                                                    |

## Behavior flow

1. Song select opens ModMenu or starts gameplay with selected mods. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:420-437`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:1095-1114`)
2. `GameScene.startGame(...)` builds scene layers, shows loader, cancels stale load work, and starts a request-id guarded pipeline. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:939-1021`)
3. Loader shows background, dim layer, beatmap data, selected mods, progress, Back, and quick settings; it enters gameplay only after `isReadyToStart`. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:38-165`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:195-225`)
4. Gameplay update advances timing, replay/cursor/autoplay, timing points, breaks, HP, objects, HUD, audio/video, skip, and result routing. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1394-1724`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1804-1915`)
5. Pause stops active playback paths and overlays `PauseMenu`; resume restores scene updates and audio/video. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2651-2705`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2842-2865`)
6. End path saves replay when eligible, loads result scene, lowers song volume, and resumes difficulty calculation. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1848-1915`)

## Source index

| System                 | Primary sources                                                                                                                                                                                                                             |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Loader                 | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt`                                                                                                                                                                    |
| Game scene             | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java`                                                                                                                                                                |
| HUD and pause          | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/GameplayHUD.kt`, `HUDSkinData.kt`, `elements/HUDBackButton.kt`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java`                                       |
| Hit objects            | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObject.java`, `GameplayHitCircle.java`, `GameplaySlider.java`, `GameplaySpinner.java`, `GameplayModernSpinner.java`, `GameObjectListener.java`                             |
| Break/countdown/skip   | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java`, `Countdown.java`, `GameScene.java`                                                                                                                        |
| Scoring/audio/settings | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java`, `GameObjectListener.java`, `GameHelper.java`, `Config.java`                                                                                              |
| Mods                   | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt`, `ModIcon.kt`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/ModsIndicator.kt`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java` |
| Values                 | `third_party/osu-droid-legacy/src/com/rian/osu/beatmap/HitWindow.kt`, `DroidHitWindow.kt`, `PreciseDroidHitWindow.kt`, `hitobject/HitObject.kt`, `Constants.java`, `StatisticV2.java`                                                       |
