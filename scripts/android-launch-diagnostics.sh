#!/usr/bin/env bash
set -euo pipefail

package_name="${1:-moe.osudroid.android}"
activity_name="${2:-crc64641d1c7d31b1c5a7.MainActivity}"
timestamp="$(date +%Y%m%d-%H%M%S)"
out_dir="/tmp/osudroid-android-launch-${timestamp}"

mkdir -p "${out_dir}"

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
