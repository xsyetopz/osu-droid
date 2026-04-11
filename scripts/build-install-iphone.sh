#!/bin/zsh

set -euo pipefail

workspace_dir="${0:A:h}/.."
app_path=""

die() {
  echo "$*" >&2
  exit 1
}

trim() {
  sed 's/^[[:space:]]*//;s/[[:space:]]*$//'
}

detect_device_id() {
  local output identifiers count

  if [[ -n "${IOS_DEVICE_ID:-}" ]]; then
    echo "$IOS_DEVICE_ID"
    return 0
  fi

  output="$(xcrun devicectl list devices 2>/dev/null)"
  identifiers="$(
    printf '%s\n' "$output" | awk '
      NR <= 2 { next }
      / connected / && / iPhone / {
        if (match($0, /[0-9A-F]{8}(-[0-9A-F]{4}){3}-[0-9A-F]{12}/)) {
          print substr($0, RSTART, RLENGTH)
        }
      }
    '
  )"
  identifiers="$(printf '%s\n' "$identifiers" | sed '/^$/d')"
  count="$(printf '%s\n' "$identifiers" | sed '/^$/d' | wc -l | tr -d ' ')"

  if [[ "$count" == "0" ]]; then
    die "No connected physical iPhone found via devicectl. Set IOS_DEVICE_ID to target a device explicitly."
  fi

  if [[ "$count" != "1" ]]; then
    echo "Multiple connected iPhones detected. Set IOS_DEVICE_ID to select one:" >&2
    printf '%s\n' "$output" >&2
    exit 1
  fi

  printf '%s\n' "$identifiers" | trim
}

read_bundle_id() {
  /usr/libexec/PlistBuddy -c 'Print :CFBundleIdentifier' "$app_path/Info.plist" | trim
}

find_app_path() {
  find "$workspace_dir/ios/build/robovm.tmp" -maxdepth 1 -type d -name '*.app' | head -n 1
}

echo "Building iPhone app bundle with RoboVM..."
(cd "$workspace_dir" && ./gradlew --no-configuration-cache ios:launchIOSDevice)

app_path="$(find_app_path)"
[[ -n "$app_path" ]] || die "Expected a built app in $workspace_dir/ios/build/robovm.tmp, but none was produced."

device_id="$(detect_device_id)"
bundle_id="$(read_bundle_id)"

echo "Target device: $device_id"
echo "Bundle identifier: $bundle_id"

if [[ "${IOS_CLEAN_INSTALL:-0}" == "1" ]]; then
  echo "Uninstalling existing app bundle..."
  xcrun devicectl device uninstall app --device "$device_id" "$bundle_id" || true
fi

echo "Installing app on device..."
xcrun devicectl device install app --device "$device_id" "$app_path"

echo "Launching app on device..."
launch_args=(xcrun devicectl device process launch --device "$device_id" --terminate-existing)
if [[ "${IOS_DEVICE_CONSOLE:-0}" == "1" ]]; then
  launch_args+=(--console)
fi
launch_args+=("$bundle_id")
"${launch_args[@]}"
