# Rewrite Architecture

## Source split
- MonoGame supplies rendering, input, and the game loop.
- .NET MAUI supplies Android/iOS lifecycle, permissions, paths, and native services.
- `osu-droid-legacy` supplies behavior, assets, online protocols, database schema, filesystem layout, and UI flow references.

## Product boundaries
- Android and iOS only.
- No desktop launcher.
- No osu-framework runtime dependency.
- Shared game behavior lives in `src/OsuDroid.Game`; platform APIs stay in `src/OsuDroid.App`.

## Runtime data layout
- `DroidGamePathLayout` owns osu!droid-compatible game folders.
- App/platform code provides only native roots (`DroidPathRoots`): core root and cache root.
- Game layout expands those roots into `Songs/`, `Skin/`, `Scores/`, `databases/`, `Log/`, and `.nomedia`.
- Database compatibility must use `DroidGamePathLayout.GetDatabasePath(buildType)` or `DroidDatabaseConstants.GetDatabasePath(...)`.

## Source ownership map
- `Scenes/MainMenu*`: main-menu state, Android timing, frame composition, controls, about dialog, and animation math.
- `Scenes/Options*`: settings catalog, selected section state, row rendering, and Android settings metrics.
- `UI/Actions`: rendering-independent UI actions, action routing, and scene-stack primitives.
- `UI/Assets`: droid asset manifest entries, provenance, and logical asset names.
- `UI/Elements`: frame snapshots, element snapshots, icon/text/corner element types.
- `UI/Geometry`: virtual viewport, points, sizes, rects, and colors.
- `UI/Style`: droid design tokens: colors, metrics, and reusable style records.
- `Runtime/*`: orchestration services, snapshots, path layout, and music-controller contracts.
- `Compatibility/*`: database, online, and multiplayer wire/schema compatibility.
- `OsuDroid.App/MonoGame/Input`: MonoGame input routing.
- `OsuDroid.App/MonoGame/Rendering`: MonoGame texture, icon, text, shape, diagnostics, and cache prewarm adapters.

## Architecture audit
- Run `python3 scripts/dev/architecture_audit.py --write docs/architecture-audit.md` before large feature branches.
- The audit is advisory. Fix source god files before adding SongSelect, BeatmapDownloader, gameplay, or import subsystems.

## UI porting
- Follow [`docs/ui-porting-guidelines.md`](ui-porting-guidelines.md) when translating legacy Android screens to shared MonoGame UI.
