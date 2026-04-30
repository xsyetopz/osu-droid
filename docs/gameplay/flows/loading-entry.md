# Loading Entry

## Loader construction

- `GameLoaderScene` stores the game scene, beatmap, selected mods, restart flag, beatmap options, last-touch time, and start state. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:24-37`)
- Normal entry uses `2000ms` fade/minimum timeouts; restart uses `500ms`. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:35-36`)
- Background uses beatmap background unless safe background is enabled, then applies black dim at `0.7` alpha. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:41-55`)

## Panels

- Main panel fades and scales in over `0.2s`; it shows epilepsy warning, title, difficulty, artist, selected mods, progress, Back outside multiplayer, and quick settings. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:57-165`)
- Beatmap quick settings expose per-set offset `-250..250ms`, persist to `beatmapOptionsTable`, and provide `-5/-1/+1/+5` step buttons. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:254-299`)
- Settings quick settings expose background brightness, storyboard, video, and scoreboard; brightness updates dim alpha and reloads storyboard/video. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:310-356`)
- Touching quick settings sets alpha to `1`; restart touches extend fade/minimum timeout to `1500ms`; idle alpha decays toward `0.5`. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:370-390`)
- Full UI values table: `docs/gameplay/contracts/ui-animation-values.md`.

## Cancel and start

- Cancel is ignored in multiplayer; solo cancel stops loading, restores song menu scene, and restarts selected preview audio at preview time. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:168-187`)
- Loader waits for `gameScene.isReadyToStart`; multiplayer skips minimum timeout, then loader fades out, dim fades to `1 - bgbrightness / 100`, HUD fades/scales in, and `gameScene.start()` runs. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:195-225`)
