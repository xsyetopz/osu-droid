#!/bin/sh
set -eu

if [ "${1:-}" = "--help" ]; then
  cat <<'EOF'
Usage: scripts/doctor-ios-device.sh IOS_DEVICE_ID=<device-id>

Collects CoreDevice health evidence for the connected iPhone and writes:
  /tmp/osudroid-devicectl-apps.log
  /tmp/osudroid-devicectl-apps.json
EOF
  exit 0
fi

IOS_DEVICE_ID="${IOS_DEVICE_ID:-}"

if [ -z "$IOS_DEVICE_ID" ]; then
  echo "IOS_DEVICE_ID is required. Example: IOS_DEVICE_ID=<device-id> ./scripts/doctor-ios-device.sh" >&2
  exit 1
fi

LOG_PATH="/tmp/osudroid-devicectl-apps.log"
JSON_PATH="/tmp/osudroid-devicectl-apps.json"

rm -f "$LOG_PATH" "$JSON_PATH"

if xcrun devicectl device info apps \
  --device "$IOS_DEVICE_ID" \
  --timeout 20 \
  --verbose \
  --log-output "$LOG_PATH" \
  --json-output "$JSON_PATH"; then
  echo "CoreDevice probe succeeded."
  exit 0
fi

echo "CoreDevice probe failed."
echo "Log:  $LOG_PATH"
echo "JSON: $JSON_PATH"

if [ -f "$LOG_PATH" ]; then
  echo "--- $LOG_PATH ---"
  sed -n '1,160p' "$LOG_PATH"
fi

if [ -f "$JSON_PATH" ]; then
  echo "--- $JSON_PATH ---"
  sed -n '1,200p' "$JSON_PATH"
fi

exit 1
