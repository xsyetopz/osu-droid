# Validation Checklist

## Repository scope

- Only files under `docs/gameplay/` changed for this reference pass.
- No gameplay runtime code changed.
- No runtime assets moved or copied.

## Documentation checks

- Every behavior claim cites source path and line range.
- Links from `docs/gameplay/README.md` resolve to existing files.
- Source citations resolve to files under `third_party/osu-droid-legacy/src`.
- Value contracts include numeric formulas for timing, geometry, UI animation, score, HP, ranks, and skip.
- No stale planning terms are introduced.

## Gameplay parity checks for later implementation

- Loader shows beatmap background, dim overlay, metadata panel, selected mods, progress, Back, and quick settings before gameplay starts. Sources: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:38-165`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:254-356`.
- GameScene creates and updates background, midground, foreground, and HUD layers. Source: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:138-325`.
- Pause is hold-to-pause through HUD back button, not a tap-only menu action. Source: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:54-131`.
- Default HUD includes HP/progress, score, accuracy, leaderboard, combo, offset/UR/FPS diagnostics, and back hold button. Sources: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/HUDSkinData.kt:27-96`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/GameplayHUD.kt:188-219`.
- Hit circles, sliders, and spinners report through `GameObjectListener` into `GameScene` scoring methods. Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObjectListener.java:12-25`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2268-2475`.
- Hit windows, preempt, object radius, score formulas, HP deltas, and loader/HUD animation constants match the tables in `docs/gameplay/contracts/timing-geometry-values.md`, `docs/gameplay/contracts/scoring-values.md`, and `docs/gameplay/contracts/ui-animation-values.md`.
- Skip is governed by next-object timing and multiplayer skip request behavior. Source: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1941-2064`.
- ModMenu owns selected mod state and hands it to gameplay start; multiplayer rooms constrain that state. Sources: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:42-68`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:454-539`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:1095-1114`.

## Commands for this pass

```sh
/opt/homebrew/bin/rtk --ultra-compact git diff --stat
python3 scripts/dev/check-stale-terms.py
```
