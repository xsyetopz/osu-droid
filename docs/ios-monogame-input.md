# iOS MonoGame Input Notes

## Text input ownership
- iOS runs through `Platforms/iOS/AppDelegate.cs`, not the MAUI `MainPage` path.
- Platform services that the shared game needs must be attached in `AppDelegate` before `OsuDroidMonoGame` starts.
- `MainPage` service wiring is Android/MAUI-shell wiring only; it does not affect the current iOS MonoGame host.

## Search keyboard path
- Shared game code requests text through `ITextInputService`.
- iOS implements that request with a native `UITextField` attached to the active key window/root view.
- The field is transparent and positioned over the virtual search bounds, then `BecomeFirstResponder()` opens the system keyboard.
- `EditingChanged` updates the shared query text.
- keyboard Return submits the query, resigns first responder, hides the field, and clears search focus.

## Failure mode we hit
- Symptom: tapping Beatmap Downloader search highlighted the SearchBar, but no keyboard or dialog opened.
- Cause: `OsuDroidGameCore` on iOS was created directly in `AppDelegate`, so the `PlatformTextInputService` created in `MainPage` was never attached.
- Result: `UiAction.DownloaderSearchBox` reached shared game state, but the active text input service was still `NoOpTextInputService`.
- Fix: retain `PlatformTextInputService` in `AppDelegate`, call `Attach()`, and pass it to `core.AttachPlatformServices(...)` before creating `OsuDroidMonoGame`.

## Rules for future platform services
- Check both host paths before assuming a platform service is wired:
  - Android/MAUI: `MainPage`.
  - iOS/MonoGame: `Platforms/iOS/AppDelegate.cs`.
- Keep platform-native controls in `src/OsuDroid.App/Platform/*`; shared scenes should only talk to runtime interfaces.
- If a shared action changes UI state but native behavior does not happen, first verify the platform service was attached to the exact `OsuDroidGameCore` instance used by `OsuDroidMonoGame`.
- Prefer UIKit/.NET bindings over Objective-C FFI unless a UIKit API is unavailable through bindings.

