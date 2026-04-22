#!/bin/sh
set -eu

APP_PATH="${1:-}"
FIX_MODE="${2:-}"
IOS_CODESIGN_KEY="${IOS_CODESIGN_KEY:-}"
IOS_DEVELOPER_DIR="${IOS_DEVELOPER_DIR:-/Applications/Xcode_26.3.app/Contents/Developer}"
RESOLVED_CODESIGN_IDENTITY=""
export DEVELOPER_DIR="$IOS_DEVELOPER_DIR"

require_app_path() {
  if [ -z "$APP_PATH" ]; then
    echo "usage: $0 <path-to-app> [--fix]" >&2
    exit 1
  fi

  if [ ! -d "$APP_PATH" ]; then
    echo "app bundle not found: $APP_PATH" >&2
    exit 1
  fi
}

verify_codesign() {
  verification_output="$(codesign --verify --deep --strict "$APP_PATH" 2>&1 || true)"
  if [ -z "$verification_output" ]; then
    return 0
  fi

  if printf '%s\n' "$verification_output" | grep -q 'CSSMERR_TP_NOT_TRUSTED'; then
    echo "warning: codesign trust verification reported CSSMERR_TP_NOT_TRUSTED on this host" >&2
    codesign -dv "$APP_PATH" >/dev/null 2>&1
    return 0
  fi

  printf '%s\n' "$verification_output" >&2
  exit 1
}

verify_app_icon() {
  info_plist="$APP_PATH/Info.plist"

  if [ ! -f "$info_plist" ]; then
    echo "Info.plist missing from app bundle: $info_plist" >&2
    exit 1
  fi

  icon_dictionary="$(plutil -extract CFBundleIcons xml1 -o - "$info_plist" 2>/dev/null || true)"
  if [ -z "$icon_dictionary" ]; then
    echo "iOS app icon metadata missing from Info.plist" >&2
    exit 1
  fi

  for icon_file in Icon-60@2x.png Icon-60@3x.png Icon-76@2x.png Icon-83.5@2x.png; do
    if [ ! -f "$APP_PATH/$icon_file" ]; then
      echo "iOS app icon file missing from app bundle: $icon_file" >&2
      exit 1
    fi
  done
}

require_app_path

if printf '%s' "$IOS_CODESIGN_KEY" | grep -Eq '^[0-9A-Fa-f]{40}$'; then
  RESOLVED_CODESIGN_IDENTITY="$IOS_CODESIGN_KEY"
elif [ -n "$IOS_CODESIGN_KEY" ]; then
  RESOLVED_CODESIGN_IDENTITY="$(security find-identity -v -p codesigning | awk -v key="$IOS_CODESIGN_KEY" 'index($0, "\"" key "\"") && $0 !~ /CSSMERR_TP_CERT_REVOKED/ { print $2; exit }')"
else
  RESOLVED_CODESIGN_IDENTITY=""
fi

if [ "$FIX_MODE" != "" ] && [ "$FIX_MODE" != "--fix" ]; then
  echo "unknown option: $FIX_MODE" >&2
  exit 1
fi

verify_app_icon
verify_codesign
