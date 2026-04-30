# Settings Baseline

Use these defaults for gameplay screenshots and first-pass behavior.

| Setting | Baseline | Source |
| --- | --- | --- |
| Storyboard | Off by default; also disabled when brightness <= 2%. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:136-137`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:291-292`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:398-470` |
| Video | Off by default; also disabled when brightness <= 2% or no beatmap video exists. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:136-137`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:459-470` |
| Background brightness | `25%`; loader/start dim uses raw `bgbrightness`. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:155-162`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:205-213` |
| Cursor display | Hidden by default; replay or `showcursor=true` can show cursor sprites; autoplay/autopilot suppress normal cursor sprites. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:223-226`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1125-1139` |
| Scoreboard | Shown by default; `GameScene.start()` removes `HUDLeaderboard` only when false. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:335-340`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1356-1358` |
| Frame offset fix | On by default. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:223-227`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:299-300` |
| Dim hit objects | On by default. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:136-140`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:775-780` |
| Break dim change | Enabled by default because `noChangeDimInBreaks=false`. | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:136-140`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/BreakAnimator.java:110-120` |
