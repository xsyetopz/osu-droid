#!/bin/zsh

set -euo pipefail

repo_root="${0:A:h}/.."
cd "$repo_root"

if [[ ! -f "upstream-sources.lock.json" ]]; then
  echo "Missing upstream-sources.lock.json in $repo_root" >&2
  exit 1
fi

./gradlew tools:run --args="sync-ui"
