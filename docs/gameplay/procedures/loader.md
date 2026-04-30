# Procedure: Loader Entry

Use this as control-flow specification, not host-language code.

```text
procedure start_game(beatmap, mods, multiplayer_context, replay_context):
  increment request_id
  clear previous scene children and transient gameplay state
  create background, midground, foreground, HUD, pause layers
  attach loader scene over gameplay scene
  cancel stale storyboard, video, beatmap, and difficulty jobs for old request_id
  launch load pipeline for current request_id
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:939-1021`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:586-725`

```text
procedure loader_update(delta):
  render beatmap background, or menu-background when safe background is enabled
  render black dim layer with alpha 0.7
  show main panel at x=60 with alpha 0 -> 1 and scale 0.9 -> 1 over 0.2s
  show title width 700 with autoscroll speed 30
  show difficulty, artist, selected mods, progress spinner 32x32, and Back button outside multiplayer
  show quick settings panel:
    beatmap offset slider -250..250ms
    offset step buttons -5, -1, +1, +5
    background brightness slider 0..100%, default 25%
    storyboard/video/scoreboard toggles
  update progress text from load status
  if Back is pressed:
    cancel current request_id
    stop loader transition
    return to song select
  if quick settings is touched during restart:
    set fadeTimeout = 1500ms
    set minimumTimeout = 1500ms
  if quick settings is idle and alpha > 0.5 after fadeTimeout:
    alpha -= delta * 1.5
  if load pipeline marks ready:
    enable tap/start transition
  if start transition completes:
    fade main panel out over 0.1s
    dim to 1 - bgbrightness / 100 over 0.2s
    fade HUD in over 0.1s and scale HUD 0.9 -> 1 over 0.2s
    remove loader scene
    call gameplay start
```

Sources: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:38-165`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:168-187`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:195-225`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:254-356`
