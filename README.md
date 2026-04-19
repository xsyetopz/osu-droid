# osu-droid

This repository is the active `.NET 9` mobile rewrite root for osu!droid.

## Source of Truth
- original `osudroid/osu-droid`: behavior, assets, database shape, and online protocol source
- MonoGame: rendering/input/game loop
- .NET MAUI: Android/iOS app host, lifecycle, permissions, native platform services

## Layout
- `src/OsuDroid.Game`: shared runtime, compatibility services, scene logic, and storage contracts
- `src/OsuDroid.App`: MAUI mobile host plus MonoGame surface
- `tests/OsuDroid.Game.Tests`: shared tests
- `third_party/`: local gitignored study checkouts

## Bootstrap
1. Clone the local study repos with `scripts/bootstrap-third-party.sh`.
2. Install the required .NET workloads:
   - `dotnet workload install maui-android`
   - `dotnet workload install maui-ios`
3. Restore and build:
   - `dotnet restore OsuDroid.sln`
   - `dotnet build OsuDroid.sln`

## iPhone Device Build
- Confirm the connected device ID with `xcrun xcdevice list`.
- Confirm signing with `security find-identity -v -p codesigning`.
- The current `.NET iOS` workload requires Xcode 26.3. Keep Xcode 26.4+ installed if needed, but install Xcode 26.3 side-by-side as `/Applications/Xcode_26.3.app`.
- If the certificate display suffix disagrees with provisioning, trust the provisioning profile `TeamIdentifier` and embedded certificate SHA1. Some machines show stale team suffixes in Keychain display names.
- Audit profiles:
  - `IOS_DEVICE_ID=<device-id> IOS_CODESIGN_KEY=<sha1> scripts/ios-signing.sh audit`
- Create/update an Apple Developer **iOS App Development** profile for bundle `moe.osudroid`, include the connected iPhone UDID, and select the Apple Development certificate SHA1 printed by `security find-identity`.
- Build/install/launch with:
  - `make install-ios IOS_DEVELOPER_DIR=/Applications/Xcode_26.3.app/Contents/Developer IOS_DEVICE_ID=<device-id> IOS_CODESIGN_KEY=<sha1> IOS_PROVISIONING_PROFILE="$HOME/Documents/Profiles/OsuDroid.mobileprovision"`
  - `make launch-ios IOS_DEVELOPER_DIR=/Applications/Xcode_26.3.app/Contents/Developer IOS_DEVICE_ID=<device-id>`

## Compatibility Contract
- Keep `https://osudroid.moe/api/` login/score/replay semantics compatible with osu!droid.
- Keep `https://multi.osudroid.moe` multiplayer endpoints, socket event names, and API version compatible.
- Keep the local Room SQLite database shape import-compatible with current osu!droid data.
- Do not add osu-framework, libGDX, Gradle, RoboVM, AndEngine, or desktop launchers.

### iOS visual QA with Appium

Start Appium in another shell:

```sh
appium
```

Then capture or tap the connected iPhone:

```sh
make appium-doctor
make appium-ios-screenshot IOS_DEVICE_ID=<device-id>
make appium-ios-tap IOS_DEVICE_ID=<device-id> X=<x> Y=<y>
```

Live screenshots are written to `screenshots/ios-live.png` and ignored by git. Android reference screenshots live in `screenshots/` and are documented in `screenshots/README.md`.
