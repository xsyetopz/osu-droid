# Asset Keys

## Loader and mod UI

| Key | Role | Source |
| --- | --- | --- |
| `menu-background` | Safe/fallback loader background. | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:41-47` |
| `::background` | Beatmap background texture when safe background is off. | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:41-47` |
| `back-arrow` | Loader, HUD, and ModMenu back icon. | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:38-47`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:20-29`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:81-89` |
| `warning` | Loader epilepsy warning. | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:69-89` |
| `selection-mods`, `selection-mods-over` | Song menu mods button normal/pressed textures. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:420-437` |
| `tune`, `backspace`, `search-small`, `settings` | ModMenu high-quality icons. | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:81-89` |
| `mod.iconTextureName` | Mod icon texture lookup; fallback is acronym text. | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModIcon.kt:35-78` |

## Gameplay objects

| Key | Role | Source |
| --- | --- | --- |
| `hitcircle`, `hitcircleoverlay` | Hit circle body and overlay. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayHitCircle.java:38-49` |
| `approachcircle` | Hit circle and slider approach ring. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayHitCircle.java:44-49`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySlider.java:333-349` |
| `followpoint` | Passive connection between eligible non-spinner objects. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1826-1828` |
| `sliderscorepoint` | Slider tick sprite texture. | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/game/SliderTicks.kt:79-79` |
| `spinner-background`, `spinner-circle`, `spinner-metre`, `spinner-approachcircle`, `spinner-spin`, `spinner-clear` | Classic spinner layers. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySpinner.java:61-106` |

## HUD, pause, break, countdown

| Key | Role | Source |
| --- | --- | --- |
| `scorebar-bg` | Health bar background. | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDHealthBar.kt:37-37` |
| `pause-save-replay`, `pause-continue`, `pause-retry`, `pause-back` | Pause menu items. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java:80-91` |
| `fail-background`, `pause-overlay` | Fail/pause overlay background. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java:93-112` |
| `play-warningarrow` | Four warning arrows during final break second. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:39-61` |
| `section-pass`, `section-fail` | Mid-break pass/fail mark. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:85-93` |
| `ranking-*-small` | Current ranking mark during break. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:101-108`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:122-124` |
| `ready`, `count3`, `count2`, `count1`, `go` | Countdown sprites. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/Countdown.java:40-108` |
