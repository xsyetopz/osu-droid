# osu-droid

This repository is the active `.NET 8` mobile rewrite root for osu!droid.

## Source of Truth
- `ppy/osu-framework`: runtime, graphics, audio, input, and mobile app structure
- `ppy/osu`: behavior, route separation, and strings/localisation keys
- original `osudroid/osu-droid`: mobile layout cues only

## Layout
- `src/OsuDroid.Game`: shared runtime, UI, localisation, and service contracts
- `src/OsuDroid.Android`: Android app head
- `src/OsuDroid.iOS`: iOS app head
- `tests/OsuDroid.Game.Tests`: shared tests
- `third_party/`: local gitignored study checkouts

## Bootstrap
1. Clone the local study repos with `scripts/bootstrap-third-party.sh`.
2. Install the required .NET workloads:
   - `dotnet workload install android`
   - `dotnet workload install ios`
3. Restore and build:
   - `dotnet restore OsuDroid.sln`
   - `dotnet build OsuDroid.sln`
