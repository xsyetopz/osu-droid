#!/bin/sh
set -eu

APK_PATH="${1:-src/OsuDroid.Android/bin/Debug/net8.0-android/moe.osudroid.android-Signed.apk}"

if [ ! -f "$APK_PATH" ]; then
  echo "APK not found: $APK_PATH" >&2
  exit 1
fi

TMP_DIR="$(mktemp -d /tmp/osudroid-bass-check.XXXXXX)"
trap 'rm -rf "$TMP_DIR"' EXIT

extract_and_verify() {
  abi="$1"
  name="$2"
  expected_size="$3"
  expected_needed="$4"
  output_path="$TMP_DIR/${abi}-${name}"

  unzip -p "$APK_PATH" "lib/${abi}/${name}" > "$output_path"

  actual_size="$(wc -c < "$output_path" | tr -d ' ')"
  if [ "$actual_size" != "$expected_size" ]; then
    echo "Unexpected size for ${abi}/${name}: got ${actual_size}, expected ${expected_size}" >&2
    exit 1
  fi

  if ! llvm-readelf -d "$output_path" | grep -F "$expected_needed" >/dev/null; then
    echo "Missing expected dependency ${expected_needed} in ${abi}/${name}" >&2
    exit 1
  fi
}

check_no_duplicates() {
  abi="$1"
  name="$2"
  count="$(unzip -Z1 "$APK_PATH" | grep -c "^lib/${abi}/${name}$")"
  if [ "$count" != "1" ]; then
    echo "Expected exactly one ${abi}/${name} entry in APK, found ${count}" >&2
    exit 1
  fi
}

check_no_duplicates arm64-v8a libbass.so
check_no_duplicates arm64-v8a libbass_fx.so
check_no_duplicates arm64-v8a libbassmix.so
check_no_duplicates x86 libbass.so
check_no_duplicates x86 libbass_fx.so
check_no_duplicates x86 libbassmix.so

extract_and_verify arm64-v8a libbass.so 322992 libOpenSLES.so
extract_and_verify arm64-v8a libbass_fx.so 104344 libbass.so
extract_and_verify arm64-v8a libbassmix.so 59072 libbass.so
extract_and_verify x86 libbass.so 342428 libOpenSLES.so
extract_and_verify x86 libbass_fx.so 129300 libbass.so
extract_and_verify x86 libbassmix.so 67288 libbass.so

echo "Android BASS native libraries match the osu.Framework.Android AAR payload."
