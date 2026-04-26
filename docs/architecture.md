# Rewrite Architecture

## Source split
- MonoGame supplies rendering, input, and the game loop.
- .NET MAUI supplies Android/iOS lifecycle, permissions, paths, and native services.
- `third_party/osu-droid-legacy` supplies behavior, assets, online protocols, database schema, filesystem layout, and UI flow references.

## Product boundaries
- Android and iOS only.
- No desktop launcher.
- No osu-framework runtime dependency.
- Scene orchestration lives in `src/OsuDroid.Game`; bounded shared subsystems live in `src/OsuDroid.Game.{Beatmaps,Compatibility,Runtime,UI}`; platform APIs stay in `src/OsuDroid.App`.

## Runtime data layout
- `DroidGamePathLayout` owns osu!droid-compatible game folders.
- App/platform code provides only native roots (`DroidPathRoots`): core root and cache root.
- Game layout expands those roots into `Songs/`, `Skin/`, `Scores/`, `databases/`, `Log/`, and `.nomedia`.
- Database compatibility must use `DroidGamePathLayout.GetDatabasePath(buildType)` or `DroidDatabaseConstants.GetDatabasePath(...)`.

## Source ownership map
- `Scenes/MainMenu*`: main-menu state, Android timing, frame composition, controls, about dialog, and animation math.
- `Scenes/Options*`: settings catalog, selected section state, row rendering, and Android settings metrics.
- `OsuDroid.Game.UI/Actions`: rendering-independent UI actions, action routing, and scene-stack primitives.
- `OsuDroid.Game.UI/Assets`: droid asset manifest entries, provenance, and logical asset names.
- `OsuDroid.Game.UI/Elements`: element snapshots, icon/text/corner element types.
- `OsuDroid.Game.UI/Frames`: rendering-independent frame snapshots.
- `OsuDroid.Game.UI/Geometry`: virtual viewport, points, sizes, rects, and colors.
- `OsuDroid.Game.UI/Input`: platform-neutral text-input requests.
- `OsuDroid.Game.UI/Scrolling`: reusable scroll and fling state.
- `OsuDroid.Game.UI/Style`: droid design tokens: colors, metrics, and reusable style records.
- `OsuDroid.Game.Runtime/Audio`: beatmap preview, menu music, and menu sound contracts/state.
- `OsuDroid.Game.Runtime/Settings`: setting values, stores, backups, and runtime settings.
- `OsuDroid.Game.Runtime/Timing`: shared game-clock state.
- `OsuDroid.Game.Runtime/Paths`: osu!droid-compatible path layout and native root records.
- `OsuDroid.Game.Runtime/Diagnostics`: diagnostics shared by runtime callers.
- `OsuDroid.Game.Beatmaps/*`: beatmap parsing, library storage, import, online download, and difficulty math.
- `OsuDroid.Game.Compatibility/*`: database, online, and multiplayer wire/schema compatibility.
- `OsuDroid.App/MonoGame/Input`: MonoGame input routing.
- `OsuDroid.App/MonoGame/Rendering`: MonoGame texture, icon, text, shape, diagnostics, and cache prewarm adapters.

## Architecture audit
- Run `python3 scripts/dev/architecture_audit.py --write docs/architecture-audit.md` before large feature branches.
- The audit is advisory. Keep flagged source files at zero before adding large scenes, gameplay, or import subsystems.

## UI porting
- Follow [`docs/ui-porting-guidelines.md`](ui-porting-guidelines.md) when translating Android reference screens to shared MonoGame UI.

## iOS platform services
- iOS currently uses a direct MonoGame `UIApplicationDelegate` host, so shared-game platform services must be attached in `Platforms/iOS/AppDelegate.cs`.
- Android/MAUI host wiring in `MainPage` does not affect the current iOS game instance.
- See [`docs/ios-monogame-input.md`](ios-monogame-input.md) for the text-input trap and keyboard wiring pattern.
