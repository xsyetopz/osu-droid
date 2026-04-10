# Official libGDX Spike

## Purpose
Create a clean reference rewrite project that follows libGDX's documented Android+iOS structure before gameplay migration begins.

## Project generator
- Use `gdx-liftoff`
- Language: `Java`
- Platforms: `Android`, `iOS`, `LWJGL3`
- Optional additions: `Freetype`, `Tools`, `Add GUI Assets`, `Add README`
- Skip physics, AI, ECS, Box2D, controllers, and other optional libraries in the first spike

## Expected module shape
- `core`
- `android`
- `ios`
- `lwjgl3`
- `assets`
- `gradle` wrapper and normal libGDX build files

## One-time bootstrap
1. Install Temurin JDK 17.
2. Use Android Studio once to import the generated project and confirm the Android SDK/NDK setup that libGDX expects.
3. Confirm the iOS launcher, signing, and provisioning flow from Xcode CLI.
4. Keep day-to-day editing in VS Code plus Gradle/Xcode CLI after the initial sanity check.

## Launcher expectations
- Android: one thin launcher activity
- iOS: one thin launcher that delegates into shared libGDX `core`
- Desktop: keep `lwjgl3` for quick iteration, replay validation, and rendering/debug checks

## Initial spike acceptance
- Desktop window launches into a trivial `Game`/`Screen` flow
- Android build installs and runs on the Samsung S24
- iOS build installs and runs on the iPhone 14
- Assets load through `AssetManager`
- Platform-specific services are represented by interfaces in `core` and stub implementations in each launcher module

## After the spike
- Start with parser, timing, judgement, score, and replay systems
- Keep the first gameplay implementation deterministic and test-heavy
- Pull behavior from the legacy dossiers instead of porting AndEngine structure
