#!/bin/sh
set -eu

if [ "${1:-}" = "--help" ] || [ "$#" -ne 1 ]; then
  cat <<'EOF'
Usage: scripts/verify-ios-install.sh <app-path>

Verifies the installed iOS app bundle version matches the built .app.
Required env: IOS_DEVICE_ID, IOS_BUNDLE_ID.
Optional env: IOS_DEVELOPER_DIR.
EOF
  if [ "${1:-}" = "--help" ]; then
    exit 0
  fi
  exit 1
fi

APP_PATH="$1"
IOS_DEVICE_ID="${IOS_DEVICE_ID:-}"
IOS_BUNDLE_ID="${IOS_BUNDLE_ID:-moe.osudroid}"
IOS_DEVELOPER_DIR="${IOS_DEVELOPER_DIR:-/Applications/Xcode_26.3.app/Contents/Developer}"
JSON_PATH="/tmp/osudroid-devicectl-apps.json"
LOG_PATH="/tmp/osudroid-devicectl-apps.log"
export DEVELOPER_DIR="$IOS_DEVELOPER_DIR"

if [ -z "$IOS_DEVICE_ID" ]; then
  echo "IOS_DEVICE_ID is required." >&2
  exit 1
fi

if [ ! -d "$APP_PATH" ]; then
  echo "iOS app bundle not found: $APP_PATH" >&2
  exit 1
fi

built_version="$(/usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' "$APP_PATH/Info.plist")"
if [ -z "$built_version" ]; then
  echo "Built app CFBundleVersion is empty: $APP_PATH/Info.plist" >&2
  exit 1
fi

rm -f "$JSON_PATH" "$LOG_PATH"

if ! xcrun devicectl device info apps \
  --device "$IOS_DEVICE_ID" \
  --timeout 20 \
  --json-output "$JSON_PATH" \
  --log-output "$LOG_PATH" >/dev/null; then
  echo "Failed to query installed iOS apps." >&2
  echo "Log:  $LOG_PATH" >&2
  echo "JSON: $JSON_PATH" >&2
  exit 1
fi

installed_version="$(python3 - "$JSON_PATH" "$IOS_BUNDLE_ID" <<'PY'
import json
import sys

json_path, bundle_id = sys.argv[1], sys.argv[2]
with open(json_path, encoding="utf-8") as file:
    payload = json.load(file)

for app in payload.get("result", {}).get("apps", []):
    if app.get("bundleIdentifier") == bundle_id:
        print(app.get("bundleVersion", ""))
        sys.exit(0)

sys.exit(1)
PY
)" || {
  echo "Installed iOS app not found: $IOS_BUNDLE_ID" >&2
  exit 1
}

if [ "$installed_version" != "$built_version" ]; then
  echo "Installed iOS app is stale: built CFBundleVersion=$built_version installed bundleVersion=$installed_version" >&2
  echo "App: $IOS_BUNDLE_ID" >&2
  echo "Log: $LOG_PATH" >&2
  exit 1
fi

echo "Installed iOS app verified: $IOS_BUNDLE_ID bundleVersion=$installed_version"
