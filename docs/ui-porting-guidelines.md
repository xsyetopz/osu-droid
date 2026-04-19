# UI Porting Guidelines

## Source order
- Use `third_party/osu-droid-legacy/res` as the primary UI source for colors, text sizes, spacing, radii, drawable shapes, and control semantics.
- Use Android Java/Kotlin runtime math for dynamic scene geometry.
- Use Android scene code for timing behavior: easing curves, idle delays, route delays, touch tint, and animated state transitions.
- Use Android screenshots for calibration only after source math is understood. Freeze accepted layers and recalibrate only the broken layer.
- Use iOS screenshots for final verification on the connected device.

## Cross-platform translation
- Android XML values are visual tokens. Keep them exact unless a source file proves otherwise.
- Raw Android layout behavior can be translated when it does not scale to iOS/Android landscape. Example: a `match_parent` seekbar can become a trailing control column while keeping Android track height, thumb size, colors, and radii.
- Reused osu!droid image assets are owned runtime assets under `src/OsuDroid.App/Resources/Raw/assets/droid`; `third_party/osu-droid-legacy` remains provenance/reference only.
- Use lowercase/kebab-case physical asset paths and PascalCase C# constants/types. Do not use ppy `osu-resources`.
- Keep shared layout decisions in `src/OsuDroid.Game`; keep platform-specific rendering details in the MAUI/MonoGame host.

## Settings UI values currently mapped
- Root background: `#13131A`.
- App bar: `#1E1E2E`.
- Row/category background: `#161622`.
- Selected sidebar/input/seekbar background: `#363653`.
- Accent/progress: `#F37373`.
- Summary text: `#FFB2B2CC`.
- Sidebar item width: `200dp`; padding: `12dp`; drawable padding: `12dp`; selected radius: `15dp`.
- Row vertical/horizontal padding: `18dp` unless a source layout overrides one axis.
- Category top margin: `12dp`; category header padding: `12dp`.
- `rounded_rect`: all corners `14dp`.
- `rounded_half_top`: top corners `14dp`, bottom corners square.
- `rounded_half_bottom`: bottom corners `14dp`, top corners square.
- Seekbar track: `6dp` high, `12dp` radius, `#363653` background, `#F37373` progress.
- Seekbar thumb: `16dp x 16dp`, `12dp` radius, white.

## Main menu behavior currently mapped
- `MainScene.java` and `MainMenu.kt` drive menu timing/behavior.
- Cookie opens from center to the left over `0.3s` with exponential-out easing.
- Menu buttons enter from `100px` left of their final Android source position, fade to `0.9`, and settle over `0.5s` with elastic-out easing.
- Visible menu auto-collapses after `10000ms` idle.
- Menu buttons collapse left by `50px` and fade out over `1s` with exponential-out easing.
- Cookie returns to center over `1s` with bounce-out easing.
- Cookie heartbeat uses a `1000ms` beat: scale `1.0 -> 1.07` over `0.9 beat`, then `1.07 -> 1.0` over `0.07 beat`.
- Cookie overlay is a separate logo copy above the main logo with static `1.07` scale and `0.2` alpha; heartbeat scales only the main logo.
- Exit delays the route until Android's `3s` fade-out finishes.
- Song Select back-to-main scenery is `MainScene.loadTimingPoints`: attach the selected beatmap/menu background behind `lastBackground`, then fade `lastBackground` from `1 -> 0` over `1.5s`.
- Press tint belongs to Android source elements that call `setColor(0.7f, 0.7f, 0.7f)`: menu buttons, music buttons, downloader tab. Cookie press does not tint.
- Accepted static main-menu placement is frozen: logo/menu button positions should not move while polishing music, badge, version, or animation layers.

## Rendering rules
- Render Android shape drawables with anti-aliasing. Do not approximate rounded UI with jagged scanline rectangles.
- Middle preference rows are allowed to have sharp corners because Android stacks them between rounded category/header and bottom rows.
- Rounded output differences between iOS and Android are bugs unless the underlying Android resource has square corners.

## Validation rule
- After visual UI changes, run tests, build, install, and launch on the connected iPhone before reporting completion.
- Minimum commands:
  - `dotnet test tests/OsuDroid.Game.Tests/OsuDroid.Game.Tests.csproj --no-restore -nr:false -v:minimal`
  - `dotnet build OsuDroid.sln --warnaserror -nr:false -v:minimal`
  - `make install-ios ...`
  - `make launch-ios ...`
