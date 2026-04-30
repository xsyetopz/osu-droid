# UI and Animation Values

## Loader values

| Element | Value / behavior | Source |
| --- | --- | --- |
| Background image | `::background`, or `menu-background` when safe beatmap background is enabled | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:41-47` |
| Initial dim | black overlay `alpha=0.7` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:49-55` |
| Main panel initial state | `alpha=0`, `scaleX=0.9`, `scaleY=0.9` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:57-68` |
| Main panel enter | `fadeIn(0.2, OutCubic)`, `scaleTo(1, 0.2, OutCubic)` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:57-68` |
| Beatmap text x | `60` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:92-129` |
| Beatmap title width | `700` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:99-107` |
| Title autoscroll speed | `30` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:99-107` |
| Loader progress size | `32 x 32` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:131-142` |
| Back icon size | `28 x 28` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:144-160` |
| Start dim target | `1 - bgbrightness / 100` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:195-225` |
| Start panel exit | main panel `fadeOut(0.1, OutExpo)` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:195-225` |
| Start dim transition | `fadeTo(target, 0.2)` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:195-225` |
| HUD enter | `alpha=0`, scale `0.9 -> 1` over `0.2 OutCubic`, fade in `0.1 OutExpo` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:214-224` |

## Loader quick settings

| Control | Values | Source |
| --- | --- | --- |
| Beatmap offset slider | min `-250`, max `250`, value/default from `beatmapOptions.offset`, format `Nms` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:254-272` |
| Offset step buttons | `-5`, `-1`, `+1`, `+5`; button height `42`, padding `(12,0,24,0)`, icon height `20` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:274-298` |
| Background brightness slider | key `bgbrightness`, default `25`, min `0`, max `100`, format `N%` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:310-338` |
| Brightness drag stop | dim returns to `0.7` over `0.1` when not starting | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:316-338` |
| Brightness change | updates config, reloads storyboard/video, sets dim alpha to `1 - value / 100` when not starting | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:316-338` |
| Storyboard checkbox | key `enableStoryboard`; reloads storyboard on change | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:340-345` |
| Video checkbox | key `enableVideo`; reloads video on change | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:347-352` |
| Scoreboard checkbox | key `showscoreboard` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:354-356` |
| Restart touch timeout | on touch during restart: `fadeTimeout=1500ms`, `minimumTimeout=1500ms` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:370-390` |
| Quick settings fade | if `alpha > 0.5` and elapsed exceeds timeout, `alpha -= delta * 1.5` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:384-390` |

## HUD back hold button

| Value | Formula / constant | Source |
| --- | --- | --- |
| Required hold | `Config.getBackButtonPressTime()`, default preference `300ms` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:16-21`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:223-236` |
| Button size | `72 x 72` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:135-137` |
| Arrow texture | `back-arrow` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:22-29` |
| Arrow relative size | `0.6 x 0.6` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:22-29` |
| Front circle color | `0xFF002626` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:42-51` |
| Front circle size | `0.95 x 0.95` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:42-51` |
| Progress | `holdDurationMs / requiredPressTimeMs`, or `0` when required is `0` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:54-67` |
| Scale | `1 + progress / 2` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:54-67` |
| Alpha | interpolate hold time from `0.25` to `0.5`, `OutCubic` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:54-67` |
| Press growth | `holdDurationMs += deltaSeconds * 1000` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:83-99` |
| Release decay | `holdDurationMs -= deltaSeconds * 1000 * 2` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:83-99` |
| Pause trigger | when hold reaches required time: clear pressed state, call `gameScene.pause()` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:83-99` |
| Move cancel | cancel press when local touch leaves `0..width` or `0..height` | `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:104-131` |

