# Repository Guidance

## Rewrite Direction
- This repository is the active `.NET 8` mobile rewrite root for osu!droid.
- Runtime and platform structure follow `ppy/osu-framework`.
- Behavior and strings follow `ppy/osu`.
- Mobile layout cues may be studied from a local gitignored checkout of the original `osudroid/osu-droid`.

## Architecture Defaults
- Android and iOS only. Do not add a desktop target.
- Keep platform-specific APIs in the mobile heads or thin adapters.
- Keep shared game code in `src/OsuDroid.Game`.
- Do not reintroduce libGDX, Gradle, RoboVM, AndEngine, or the old Android app architecture.
- Follow `.editorconfig` for C# style and [`docs/csharp-guidelines.md`](docs/csharp-guidelines.md) for repo structure.

## Local Study Sources
- `third_party/osu-framework`
- `third_party/ppy-osu`
- `third_party/osu-droid-legacy`

Prepare them with `scripts/bootstrap-third-party.sh`.
