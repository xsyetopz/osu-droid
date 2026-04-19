#!/bin/sh
set -eu

PROJECT="src/OsuDroid.App/OsuDroid.App.csproj"
CONFIGURATION="${CONFIGURATION:-Debug}"
TARGET_FRAMEWORK="${TARGET_FRAMEWORK:-net9.0-ios}"
RUNTIME_IDENTIFIER="${RUNTIME_IDENTIFIER:-ios-arm64}"
APP_NAME="${APP_NAME:-OsuDroid.App.app}"
ROOT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
IOS_DEVELOPER_DIR="${IOS_DEVELOPER_DIR:-/Applications/Xcode_26.3.app/Contents/Developer}"
IOS_PROVISIONING_PROFILE="${IOS_PROVISIONING_PROFILE:-}"
IOS_CODESIGN_KEY="${IOS_CODESIGN_KEY:-}"
IOS_TEAM_ID="${IOS_TEAM_ID:-}"
IOS_BUNDLE_ID="${IOS_BUNDLE_ID:-moe.osudroid}"
REQUIRED_XCODE_MAJOR_MINOR="${REQUIRED_XCODE_MAJOR_MINOR:-26.3}"
IOS_DEVICE_ID="${IOS_DEVICE_ID:-}"
IOS_SIGNING_HELPER="$ROOT_DIR/scripts/ios-signing.sh"

export DEVELOPER_DIR="$IOS_DEVELOPER_DIR"

require_value() {
  value="$1"
  name="$2"
  example="$3"

  if [ -z "$value" ]; then
    echo "${name} is required." >&2
    echo "Set it explicitly. Example: ${example}" >&2
    exit 1
  fi
}

require_xcode() {
  if [ ! -d "$IOS_DEVELOPER_DIR" ]; then
    echo "IOS_DEVELOPER_DIR does not exist: $IOS_DEVELOPER_DIR" >&2
    echo "Install Xcode ${REQUIRED_XCODE_MAJOR_MINOR} side-by-side or pass IOS_DEVELOPER_DIR=/path/to/Xcode.app/Contents/Developer." >&2
    exit 1
  fi

  version="$(xcodebuild -version | sed -n 's/^Xcode //p' | head -n 1)"
  case "$version" in
    "$REQUIRED_XCODE_MAJOR_MINOR"|"$REQUIRED_XCODE_MAJOR_MINOR".*) ;;
    *)
      echo ".NET iOS workload requires Xcode ${REQUIRED_XCODE_MAJOR_MINOR}; selected Xcode is ${version:-unknown}." >&2
      echo "Selected DEVELOPER_DIR: $IOS_DEVELOPER_DIR" >&2
      exit 1
      ;;
  esac
}

require_device() {
  if [ -z "$IOS_DEVICE_ID" ]; then
    return 0
  fi

  if ! xcrun xcdevice list | grep -F "$IOS_DEVICE_ID" >/dev/null; then
    echo "IOS_DEVICE_ID not visible through selected Xcode: $IOS_DEVICE_ID" >&2
    echo "Selected DEVELOPER_DIR: $IOS_DEVELOPER_DIR" >&2
    exit 1
  fi
}

resolve_signing_from_profile() {
  if [ -z "$IOS_PROVISIONING_PROFILE" ]; then
    return 0
  fi

  if [ ! -f "$IOS_PROVISIONING_PROFILE" ]; then
    echo "IOS_PROVISIONING_PROFILE not found: $IOS_PROVISIONING_PROFILE" >&2
    exit 1
  fi

  signing_settings="$("$IOS_SIGNING_HELPER" resolve-build-settings "$IOS_PROVISIONING_PROFILE" "$IOS_BUNDLE_ID" "$IOS_DEVICE_ID" "$IOS_CODESIGN_KEY")"
  CODESIGN_PROVISION="$(printf '%s\n' "$signing_settings" | sed -n 's/^CODESIGN_PROVISION=//p')"
  CODESIGN_TEAM_ID="$(printf '%s\n' "$signing_settings" | sed -n 's/^CODESIGN_TEAM_ID=//p')"
  CODESIGN_KEY="$(printf '%s\n' "$signing_settings" | sed -n 's/^CODESIGN_KEY=//p')"

  if [ -z "$CODESIGN_PROVISION" ] || [ -z "$CODESIGN_TEAM_ID" ] || [ -z "$CODESIGN_KEY" ]; then
    echo "Failed to resolve signing settings from IOS_PROVISIONING_PROFILE." >&2
    exit 1
  fi

  IOS_TEAM_ID="$CODESIGN_TEAM_ID"
  IOS_CODESIGN_KEY="$CODESIGN_KEY"
}

print_app_path() {
  primary_path="src/OsuDroid.App/bin/${CONFIGURATION}/${TARGET_FRAMEWORK}/${RUNTIME_IDENTIFIER}/${APP_NAME}"
  if [ -d "$primary_path" ]; then
    printf '%s\n' "$primary_path"
    return 0
  fi

  search_root="src/OsuDroid.App/bin/${CONFIGURATION}/${TARGET_FRAMEWORK}/${RUNTIME_IDENTIFIER}"
  fallback_path="$(find "$search_root" -type d -name "${APP_NAME}" 2>/dev/null | head -n 1 || true)"
  if [ -n "$fallback_path" ]; then
    printf '%s\n' "$fallback_path"
    return 0
  fi

  echo "Unable to find ${APP_NAME}. Build the iOS device target first." >&2
  return 1
}

if [ "${1:-}" = "--print-app-path" ]; then
  print_app_path
  exit 0
fi

require_value "$IOS_CODESIGN_KEY" "IOS_CODESIGN_KEY" "IOS_CODESIGN_KEY='Apple Development: Your Name (TEAMID)' ./scripts/build-ios-device.sh"
if [ -z "$IOS_PROVISIONING_PROFILE" ]; then
  require_value "$IOS_TEAM_ID" "IOS_TEAM_ID" "IOS_TEAM_ID=TEAMID ./scripts/build-ios-device.sh"
fi
require_xcode
require_device
resolve_signing_from_profile

if [ -n "${CODESIGN_PROVISION:-}" ]; then
  dotnet build "$PROJECT" \
    -c "$CONFIGURATION" \
    -f "$TARGET_FRAMEWORK" \
    -nr:false \
    -p:RuntimeIdentifier="$RUNTIME_IDENTIFIER" \
    -p:NuGetAudit=false \
    -p:WarningsNotAsErrors=NU1900 \
    -p:BuildMobile=true \
    -p:MobileTarget=ios \
    -p:CodesignKey="$IOS_CODESIGN_KEY" \
    -p:CodesignTeamId="$IOS_TEAM_ID" \
    -p:CodesignProvision="$CODESIGN_PROVISION"
else
  dotnet build "$PROJECT" \
    -c "$CONFIGURATION" \
    -f "$TARGET_FRAMEWORK" \
    -nr:false \
    -p:RuntimeIdentifier="$RUNTIME_IDENTIFIER" \
    -p:NuGetAudit=false \
    -p:WarningsNotAsErrors=NU1900 \
    -p:BuildMobile=true \
    -p:MobileTarget=ios \
    -p:CodesignKey="$IOS_CODESIGN_KEY" \
    -p:CodesignTeamId="$IOS_TEAM_ID"
fi

app_path="$(print_app_path)"
IOS_DEVELOPER_DIR="$IOS_DEVELOPER_DIR" IOS_CODESIGN_KEY="$IOS_CODESIGN_KEY" "$ROOT_DIR/scripts/verify-ios-bundle.sh" "$app_path" --fix
