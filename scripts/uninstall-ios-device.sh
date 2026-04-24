#!/bin/sh
set -eu

if [ "${1:-}" = "--help" ]; then
  cat <<'EOF'
Usage: IOS_DEVICE_ID=<device-id> scripts/uninstall-ios-device.sh

Uninstalls the iOS app if present. Missing app is success.
Required env: IOS_DEVICE_ID.
Optional env: IOS_BUNDLE_ID, IOS_DEVELOPER_DIR.
EOF
  exit 0
fi

IOS_DEVICE_ID="${IOS_DEVICE_ID:-}"
IOS_BUNDLE_ID="${IOS_BUNDLE_ID:-moe.osudroid}"
IOS_DEVELOPER_DIR="${IOS_DEVELOPER_DIR:-/Applications/Xcode_26.3.app/Contents/Developer}"
APPS_JSON_PATH="/tmp/osudroid-devicectl-apps.json"
APPS_LOG_PATH="/tmp/osudroid-devicectl-apps.log"
UNINSTALL_JSON_PATH="/tmp/osudroid-devicectl-uninstall.json"
UNINSTALL_LOG_PATH="/tmp/osudroid-devicectl-uninstall.log"
export DEVELOPER_DIR="$IOS_DEVELOPER_DIR"

if [ -z "$IOS_DEVICE_ID" ]; then
  echo "IOS_DEVICE_ID is required." >&2
  exit 1
fi

rm -f "$APPS_JSON_PATH" "$APPS_LOG_PATH" "$UNINSTALL_JSON_PATH" "$UNINSTALL_LOG_PATH"

if ! xcrun devicectl device info apps \
  --device "$IOS_DEVICE_ID" \
  --timeout 20 \
  --json-output "$APPS_JSON_PATH" \
  --log-output "$APPS_LOG_PATH" >/dev/null; then
  echo "Failed to query installed iOS apps before uninstall." >&2
  echo "Log:  $APPS_LOG_PATH" >&2
  echo "JSON: $APPS_JSON_PATH" >&2
  exit 1
fi

if ! python3 - "$APPS_JSON_PATH" "$IOS_BUNDLE_ID" <<'PY'
import json
import sys

json_path, bundle_id = sys.argv[1], sys.argv[2]
with open(json_path, encoding="utf-8") as file:
    payload = json.load(file)

for app in payload.get("result", {}).get("apps", []):
    if app.get("bundleIdentifier") == bundle_id:
        sys.exit(0)

sys.exit(1)
PY
then
  echo "iOS app not installed: $IOS_BUNDLE_ID"
  exit 0
fi

if ! xcrun devicectl device uninstall app \
  --device "$IOS_DEVICE_ID" \
  "$IOS_BUNDLE_ID" \
  --timeout 60 \
  --json-output "$UNINSTALL_JSON_PATH" \
  --log-output "$UNINSTALL_LOG_PATH"; then
  echo "Failed to uninstall iOS app: $IOS_BUNDLE_ID" >&2
  echo "Log:  $UNINSTALL_LOG_PATH" >&2
  echo "JSON: $UNINSTALL_JSON_PATH" >&2
  exit 1
fi

echo "Uninstalled iOS app: $IOS_BUNDLE_ID"
