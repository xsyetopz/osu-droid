#!/bin/sh
set -eu

root_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
lock_file="$root_dir/upstream-sources.lock.json"
git_bin=$(command -v git 2>/dev/null || true)

if [ -z "$git_bin" ] && [ -x /opt/homebrew/bin/git ]; then
  git_bin=/opt/homebrew/bin/git
fi

if [ -z "$git_bin" ]; then
  echo "git is required but was not found in PATH" >&2
  exit 1
fi

if [ ! -f "$lock_file" ]; then
  echo "missing upstream-sources.lock.json" >&2
  exit 1
fi

mkdir -p "$root_dir/third_party"

clone_if_missing() {
  path="$1"
  remote="$2"

  if [ ! -d "$root_dir/$path/.git" ]; then
    "$git_bin" clone "$remote" "$root_dir/$path"
  fi
}

clone_if_missing "third_party/osu-droid-legacy" "https://github.com/osudroid/osu-droid.git"

echo "third_party checkouts prepared under $root_dir/third_party"
