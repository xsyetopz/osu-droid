# Pause, Resume, and Exit

## Pause entry

- HUD back button reaches pause after `Config.getBackButtonPressTime()`; progress decays at twice elapsed time when released. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:54-99`)
- `GameScene.pause()` returns if already paused, routes HUD editor back press to HUD, uses double-tap-to-exit in multiplayer, and forces game-over wind-down when already game over. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2651-2685`)
- Solo pause pauses video, stops looping samples, clears live non-auto cursors, pauses song service, sets scene ignore update, and attaches `PauseMenu` overlay. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2687-2705`)

## Pause menu

- Pause menu has save replay, continue, retry, and back items; fail menu hides Continue and may hide Save Replay for replays; normal pause hides Save Replay. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java:80-103`)
- Menu touches are modal, move over `50` cancels selection, and fade animation advances at `2.5x`. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java:44-77`)
- Continue plays `menuback` and resumes; Back resets replay id and quits; Retry stops fail sound, plays `menuhit`, and restarts. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/PauseMenu.java:139-166`)

## Resume and exit

- Resume removes overlay, clears ignore update, quits if HP is zero without NoFail, resumes video, and resumes song service with BGM volume. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2842-2865`)
- Quit restores non-gameplay touch options and screen dim, releases storyboard/video, calls exit cleanup, resets playfield scale, returns room or old scene, and resumes difficulty calculation. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2119-2167`)
- Exit cleanup purges gameplay objects/state and resumes song-menu preview from current position unless near last `10s` or beyond `98%`, otherwise from beatmap preview time. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2067-2117`)
