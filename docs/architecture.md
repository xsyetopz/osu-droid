# Rewrite Architecture

## Source split
- MonoGame supplies rendering, input, and the game loop.
- .NET MAUI supplies Android/iOS lifecycle, permissions, paths, and native services.
- `osu-droid-legacy` supplies behavior, assets, online protocols, database schema, and UI flow references.

## Product boundaries
- Android and iOS only.
- No desktop launcher.
- No osu-framework runtime dependency.

## First milestone
- MAUI-hosted MonoGame boot shell
- Legacy-style main menu first/second-menu behavior
- Compatible `osudroid.moe` login request/response path
- Compatible Room v4 SQLite schema creation
