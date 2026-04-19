# Repository Guidance

## Rewrite Direction
- This repository is the active `.NET 9` mobile rewrite root for osu!droid.
- Runtime uses .NET MAUI mobile hosting plus MonoGame rendering/input.
- Behavior, assets, online protocols, and local database compatibility follow the original `osudroid/osu-droid`.

## Architecture Defaults
- Android and iOS only. Do not add a desktop target.
- Keep platform-specific APIs in the MAUI mobile host or thin adapters.
- Keep shared game code in `src/OsuDroid.Game`.
- Do not add osu-framework, libGDX, Gradle, RoboVM, AndEngine, or the old Android app architecture.
- Follow `.editorconfig` for C# style and [`docs/csharp-guidelines.md`](docs/csharp-guidelines.md) for repo structure.

## UI Porting Rules
- Android resources in `third_party/osu-droid-legacy/res` are the source of truth for UI colors, typography, spacing, radii, drawable shapes, and control semantics.
- Reused osu!droid image assets live as owned runtime assets under `src/OsuDroid.App/Resources/Raw/assets/droid`; `third_party/osu-droid-legacy` is reference-only at runtime.
- Screenshots verify rendered output. If Android source math and iOS output diverge, freeze accepted layers and screenshot-calibrate only the broken layer against the Android reference dimensions.
- Cross-platform layout may translate Android `match_parent`/screen-class behavior when raw Android sizing is wrong on iOS/Android landscape, but visual tokens and drawable shapes must stay Android-accurate.
- Render Android shape drawables with anti-aliased output. Do not approximate rounded UI with jagged pixel scanlines.
- After visual UI edits, build, install, and launch on the connected iPhone before reporting done. See [`docs/ui-porting-guidelines.md`](docs/ui-porting-guidelines.md).

## Local Study Sources
- `third_party/osu-droid-legacy`

Prepare them with `scripts/bootstrap-third-party.sh`.
