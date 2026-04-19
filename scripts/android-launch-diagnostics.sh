#!/usr/bin/env bash
set -euo pipefail

package_name="${1:-moe.osudroid}"
activity_name="${2:-}"
timestamp="$(date +%Y%m%d-%H%M%S)"
out_dir="/tmp/osudroid-android-launch-${timestamp}"

mkdir -p "${out_dir}"

resolve_activity() {
  adb shell cmd package resolve-activity --brief "${package_name}" 2>/dev/null \
    | tr -d '\r' \
    | tail -n 1
}

if [[ -z "${activity_name}" ]]; then
  resolved_activity="$(resolve_activity || true)"
  if [[ -n "${resolved_activity}" && "${resolved_activity}" == */* ]]; then
    activity_name="${resolved_activity#*/}"
  else
    activity_name="crc64641d1c7d31b1c5a7.MainActivity"
  fi
fi

adb shell input keyevent KEYCODE_WAKEUP >/dev/null 2>&1 || true
adb shell wm dismiss-keyguard >/dev/null 2>&1 || true
adb logcat -c
adb shell am start -n "${package_name}/${activity_name}" | tee "${out_dir}/launch.txt"
sleep 5
adb shell pidof "${package_name}" | tee "${out_dir}/pid.txt" || true
adb shell dumpsys activity activities > "${out_dir}/activities.txt"
adb shell dumpsys window windows > "${out_dir}/windows.txt"
adb logcat -d > "${out_dir}/logcat.txt"

printf '%s\n' "${out_dir}"
