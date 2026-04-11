#!/bin/sh
set -eu

root_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
patch_dir="$root_dir/patches"

apply_patch_file() {
  patch_path="$1"

  if git -C "$root_dir" apply --check "$patch_path" >/dev/null 2>&1; then
    git -C "$root_dir" apply "$patch_path"
    echo "Applied $(basename "$patch_path")"
    return 0
  fi

  if git -C "$root_dir" apply --reverse --check "$patch_path" >/dev/null 2>&1; then
    echo "Already applied $(basename "$patch_path")"
    return 0
  fi

  echo "Failed to apply $(basename "$patch_path")" >&2
  return 1
}

apply_patch_file "$patch_dir/osu-framework/android-optional-nativelibs.patch"
