#!/bin/sh
set -eu

APP_NAME="${APP_NAME:-OsuDroid.iOS.app}"
CONFIGURATION="${CONFIGURATION:-Debug}"
TARGET_FRAMEWORK="${TARGET_FRAMEWORK:-net8.0-ios}"
RUNTIME_IDENTIFIER="${RUNTIME_IDENTIFIER:-ios-arm64}"
IOS_DEVICE_ID="${IOS_DEVICE_ID:-}"
IOS_BUNDLE_ID="${IOS_BUNDLE_ID:-moe.osudroid.ios}"
OPEN_XCODE="${OPEN_XCODE:-0}"

APP_PATH="src/OsuDroid.iOS/bin/${CONFIGURATION}/${TARGET_FRAMEWORK}/${RUNTIME_IDENTIFIER}/${APP_NAME}"

if [ ! -d "$APP_PATH" ]; then
  echo "Unable to find ${APP_NAME} at ${APP_PATH}. Build the iOS target first." >&2
  exit 1
fi

echo "Xcode fallback"
echo "App path:   $APP_PATH"
echo "Bundle ID:  $IOS_BUNDLE_ID"

if [ -n "$IOS_DEVICE_ID" ]; then
  echo "Device ID:  $IOS_DEVICE_ID"
fi

echo ""
echo "Use Xcode's Devices and Simulators window to confirm the app is installed and launch it on the connected iPhone."

if [ "$OPEN_XCODE" = "1" ]; then
  open -a Xcode
fi
