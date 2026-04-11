# osu-droid

This repository is now the in-place libGDX rewrite root for osu!droid.

## Layout
- `core`: shared gameplay/runtime code under `moe.osudroid`
- `android`: Android launcher and Android-only adapters
- `ios`: iOS launcher and iOS-only adapters
- `lwjgl3`: desktop development launcher
- `tools`: asset and migration utilities
- `assets`: runtime assets used by libGDX
- `assets-raw`: editable source assets that will later feed build-time packing

## Current State
- The root has been reset to a clean libGDX-style project layout.
- Legacy Android/AndEngine/Kotlin code has been removed from the working tree.
- The first runnable baseline is a bootstrap app that uses `AssetManager` and the `moe.osudroid` namespace.

## Build

Use the generated Gradle wrapper:

```sh
./gradlew build
./gradlew lwjgl3:run
./gradlew android:assembleDebug
```

The rewrite uses Java 8 source/target compatibility for the generated libGDX Android+iOS layout, while JDK 17 remains the expected installed JDK.

## VS Code

This workspace is set up for the Red Hat Java tooling stack in VS Code:

- `redhat.java`
- `vscjava.vscode-gradle`
- `vscjava.vscode-java-debug`

The Android module relies on the Java extension's experimental Android Gradle import support. If VS Code still shows unresolved Android or `:core` imports:

1. Disable `oracle.oracle-java` for this workspace.
2. Install the recommended workspace extensions.
3. Run `Java: Clean the Java Language Server Workspace`.
4. Run `Java: Import Java Projects into Workspace` or `Java: Reload Projects`.
5. Reload the VS Code window once.

Do not commit machine-specific Java or Android SDK paths into workspace settings. Keep SDK discovery in `local.properties` and your local shell environment.

## Notes
- The rewrite follows the official libGDX module layout rather than a custom root-level shared `src/`.
- Durable rewrite architecture and migration notes live under `docs/`.
