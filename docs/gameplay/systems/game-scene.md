# Game Scene System

## Load pipeline

- `startGame(...)` resets ready state and HUD editor flags, resets playfield scale, creates main/background/midground/foreground scenes, clips midground, and installs loader scene. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:939-975`)
- `loadingRequestId` guards stale work; async load calls `loadGame`, then `prepareScene` only if the request still matches; failed uncancelled loads quit. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:960-1021`)
- `cancelLoading` increments request id when invalidating start, clears job references, stops game/storyboard/video jobs, drains the old pipeline, and does not cancel multiplayer loading. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1024-1056`)

## Beatmap and media

- `loadGame` verifies file integrity, downloads remote replay files, parses from `BeatmapCache`, rejects md5 mismatch and empty object lists, removes irrelevant mods, and creates/reuses the droid playable beatmap. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:586-646`)
- Background, storyboard, and video load before audio preloading; storyboard/video are gated by raw `bgbrightness` and their config booleans. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:398-506`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:648-651`)
- Audio file must exist before `SongService.preLoad(audioFilePath, speed, pitchShift)` runs. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:671-704`)

## Scene layers

- Main scene owns `bgScene`, clipped `mgScene`, and `fgScene`; HUD is attached to engine overlay on start, not to the gameplay scene. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:945-956`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1376-1378`)
- `prepareScene` initializes statistics, mod helpers, cursors, cursor sprites, auto cursor, countdown, combo burst, mod icons, unranked badge, flashlight, and HUD. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1058-1201`)

## Start and update

- `start()` combines global and per-beatmap offsets, caps initial elapsed time before first object preempt, applies playfield scale/background, removes leaderboard when scoreboard is disabled, enables high-precision touch for live play, brightens wakelock, and attaches HUD. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1328-1378`)
- Update advances elapsed time, multiplayer live score timing, rate changes, storyboard time, replay events, cursors, flashlight, timing/effect points, breaks, HP drain, combo burst, passive objects, active objects, auto cursor, video, music, object spawn, metronome, end routing, and skip. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1394-1724`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1804-1973`)
- Passive update is HUD, break animator, and countdown. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1990-1999`)
