# Non-gameplay parity ledger

Android screenshots and `third_party/osu-droid-legacy` source are the reference specification. Close an item only after matching source behavior and validating current output.

## Progress counters

| Counter | Current | Target |
| --- | ---: | ---: |
| Architecture audit findings | 0 | 0 |
| Boundary check failures | 0 | 0 |
| Stale-term findings | 0 | 0 |
| Open non-gameplay parity groups | 2 | 0 |

## Open groups

| Area | Reference evidence | Current implementation | Remaining closure work |
| --- | --- | --- | --- |
| Options | `screenshots/android-options-*.png`, `third_party/osu-droid-legacy/res` | `src/OsuDroid.Game/Scenes/Options` | Verify every non-gameplay row string, platform path text, dropdown behavior, slider geometry, and persisted setting action. |
| Song select | `screenshots/android-song-select-*.png`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu` | `src/OsuDroid.Game/Scenes/SongSelect` | Verify row collapse, difficulty staircase, search/filter, collections, properties, scores overlays, and background fade. |

## Closed groups

| Area | Reference evidence | Closure evidence |
| --- | --- | --- |
| Main menu | `screenshots/android-main-menu-*.png`, `third_party/osu-droid-legacy/src/com/osudroid/ui/MainMenu.kt`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/MainScene.java`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/online/OnlinePanel.java` | Account panel visibility/text geometry, exit prompt layering/animation, now-playing truncation/controls, and idle waveform shutdown are covered by MainMenu tests. |
| Beatmap downloader | `screenshots/android-beatmap-downloader-*.png`, `third_party/osu-droid-legacy/src/com/osudroid/beatmaplisting`, `third_party/osu-droid-legacy/res/layout/download_fragment.xml` | Loading orb, mirror dialog, filters, details panel, download dialog, no-video actions, and download completion states are covered by BeatmapDownloader tests. |

## Closing rule

A row closes only when current behavior matches reference source plus screenshot evidence, validation passes, and any UI-visible change is installed/launched on iPhone.
