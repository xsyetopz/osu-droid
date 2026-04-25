# Non-gameplay parity ledger

Android screenshots and `third_party/osu-droid-legacy` source are the reference specification. Close an item only after matching source behavior and validating current output.

## Progress counters

| Counter | Current | Target |
| --- | ---: | ---: |
| Architecture audit findings | 0 | 0 |
| Boundary check failures | 0 | 0 |
| Stale-term findings | 0 | 0 |
| Open non-gameplay parity groups | 5 | 0 |

## Open groups

| Area | Reference evidence | Current implementation | Remaining closure work |
| --- | --- | --- | --- |
| Main menu | `screenshots/android-main-menu-*.png`, `third_party/osu-droid-legacy/src/com/osudroid/ui/MainMenu.kt` | `src/OsuDroid.Game/Scenes/MainMenu` | Verify account/profile visibility, exit prompts, now-playing, idle waveform, and beatmap-background modal state. |
| Beatmap downloader | `screenshots/android-beatmap-downloader-*.png`, `third_party/osu-droid-legacy/src/com/osudroid/beatmaplisting` | `src/OsuDroid.Game/Scenes/BeatmapDownloader` | Verify loading orb, mirror dialog, filters, details panel, downloads tab, and no-video/download completion states. |
| Options | `screenshots/android-options-*.png`, `third_party/osu-droid-legacy/res` | `src/OsuDroid.Game/Scenes/Options` | Verify every non-gameplay row string, platform path text, dropdown behavior, slider geometry, and persisted setting action. |
| Song select | `screenshots/android-song-select-*.png`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu` | `src/OsuDroid.Game/Scenes/SongSelect` | Verify row collapse, difficulty staircase, search/filter, collections, properties, scores overlays, and background fade. |
| Mod select | `screenshots/android-mod-*.png`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu` | `src/OsuDroid.Game/Scenes/ModSelect` | Section opacity, ranked footer color/text, search glyph, stat pills, vertical/horizontal scroll clamps, disabled/selected state, and marquee are covered by tests. Continue screenshot pass on preset CRUD dialog and selected-mod strip animation. |

## Closing rule

A row closes only when current behavior matches reference source plus screenshot evidence, validation passes, and any UI-visible change is installed/launched on iPhone.
