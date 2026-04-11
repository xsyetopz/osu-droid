#!/bin/sh
set -eu

PROJECT="src/OsuDroid.iOS/OsuDroid.iOS.csproj"
CONFIGURATION="${CONFIGURATION:-Debug}"
TARGET_FRAMEWORK="${TARGET_FRAMEWORK:-net8.0-ios}"
RUNTIME_IDENTIFIER="${RUNTIME_IDENTIFIER:-ios-arm64}"
APP_NAME="${APP_NAME:-OsuDroid.iOS.app}"
IOS_CODESIGN_KEY="${IOS_CODESIGN_KEY:-}"
IOS_TEAM_ID="${IOS_TEAM_ID:-}"

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

print_app_path() {
  primary_path="src/OsuDroid.iOS/bin/${CONFIGURATION}/${TARGET_FRAMEWORK}/${RUNTIME_IDENTIFIER}/${APP_NAME}"
  if [ -d "$primary_path" ]; then
    printf '%s\n' "$primary_path"
    return 0
  fi

  search_root="src/OsuDroid.iOS/bin/${CONFIGURATION}/${TARGET_FRAMEWORK}/${RUNTIME_IDENTIFIER}"
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
require_value "$IOS_TEAM_ID" "IOS_TEAM_ID" "IOS_TEAM_ID=TEAMID ./scripts/build-ios-device.sh"

dotnet build "$PROJECT" \
  -c "$CONFIGURATION" \
  -f "$TARGET_FRAMEWORK" \
  -nr:false \
  -p:RuntimeIdentifier="$RUNTIME_IDENTIFIER" \
  -p:NuGetAudit=false \
  -p:WarningsNotAsErrors=NU1900 \
  -p:CodesignKey="$IOS_CODESIGN_KEY" \
  -p:CodesignTeamId="$IOS_TEAM_ID"
print_app_path > /dev/null
