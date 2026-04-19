#!/bin/sh
set -eu

APP_NAME="${APP_NAME:-OsuDroid.App.app}"
CONFIGURATION="${CONFIGURATION:-Debug}"
TARGET_FRAMEWORK="${TARGET_FRAMEWORK:-net9.0-ios}"
RUNTIME_IDENTIFIER="${RUNTIME_IDENTIFIER:-ios-arm64}"
IOS_DEVICE_ID="${IOS_DEVICE_ID:-}"
IOS_BUNDLE_ID="${IOS_BUNDLE_ID:-moe.osudroid}"
IOS_DEVELOPER_DIR="${IOS_DEVELOPER_DIR:-/Applications/Xcode_26.3.app/Contents/Developer}"
OPEN_XCODE="${OPEN_XCODE:-0}"
export DEVELOPER_DIR="$IOS_DEVELOPER_DIR"

APP_PATH="src/OsuDroid.App/bin/${CONFIGURATION}/${TARGET_FRAMEWORK}/${RUNTIME_IDENTIFIER}/${APP_NAME}"

if [ ! -d "$APP_PATH" ]; then
  echo "Unable to find ${APP_NAME} at ${APP_PATH}. Build the iOS target first." >&2
  exit 1
fi

echo "Xcode fallback"
echo "App path:   $APP_PATH"
echo "Bundle ID:  $IOS_BUNDLE_ID"
echo "Xcode:      $IOS_DEVELOPER_DIR"

if [ -n "$IOS_DEVICE_ID" ]; then
  echo "Device ID:  $IOS_DEVICE_ID"
fi

echo ""
echo "Use Xcode's Devices and Simulators window to confirm the app is installed and launch it on the connected iPhone."

if [ "$OPEN_XCODE" = "1" ]; then
  open "$(dirname "$(dirname "$IOS_DEVELOPER_DIR")")"
fi
