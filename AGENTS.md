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

## Local Study Sources
- `third_party/osu-droid-legacy`

Prepare them with `scripts/bootstrap-third-party.sh`.
