# Procedure: Pause, Resume, Exit

```text
procedure hold_to_pause(delta, pressed):
  if pressed:
    holdDurationMs += delta * 1000
    progress = holdDurationMs / requiredPressTimeMs
    scale = 1 + progress / 2
    alpha = interpolate(holdDurationMs, 0.25 -> 0.5, OutCubic)
    set circular fill portion to progress
    scale circles and arrow to scale
    if hold duration reached:
      request gameplay pause
  else:
    holdDurationMs -= delta * 1000 * 2
    clamp holdDurationMs to 0..requiredPressTimeMs

  if pointer moves outside 0..width or 0..height:
    clear pressed state
```

Sources: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:54-99`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:104-131`

```text
procedure pause_game():
  stop gameplay update progression
  pause music and video paths
  show dim overlay and PauseMenu
  expose Continue, Retry, and Back to Menu buttons
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2651-2705`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java:80-112`

```text
procedure pause_menu_action(action):
  if action is Continue:
    hide pause menu
    resume gameplay playback and update state
  if action is Retry:
    quit current run through game-scene exit path
    restart selected beatmap
  if action is Back to Menu:
    save replay only when current run is eligible
    quit current run
    return to song select/menu path
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java:44-77`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java:123-169`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2067-2167`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2842-2865`
