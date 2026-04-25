#!/bin/sh
set -eu

root_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
lock_file="$root_dir/upstream-sources.lock.json"
osudroid_source_path="third_party/osu-droid-legacy"
language_pack_path="third_party/osu-droid-language-pack"
git_bin=$(command -v git 2>/dev/null || true)
python_bin=$(command -v python3 2>/dev/null || true)

if [ -z "$git_bin" ] && [ -x /opt/homebrew/bin/git ]; then
  git_bin=/opt/homebrew/bin/git
fi

if [ -z "$python_bin" ] && [ -x /opt/homebrew/bin/python3 ]; then
  python_bin=/opt/homebrew/bin/python3
fi

if [ -z "$git_bin" ]; then
  echo "git is required but was not found in PATH" >&2
  exit 1
fi

if [ -z "$python_bin" ]; then
  echo "python3 is required but was not found in PATH" >&2
  exit 1
fi

if [ ! -f "$lock_file" ]; then
  echo "missing upstream-sources.lock.json" >&2
  exit 1
fi

if "$git_bin" -C "$root_dir" submodule status -- "$osudroid_source_path" >/dev/null 2>&1; then
  "$git_bin" -C "$root_dir" submodule sync --recursive "$osudroid_source_path"
  "$git_bin" -C "$root_dir" submodule update --init --remote --recursive "$osudroid_source_path"
else
  if [ ! -e "$root_dir/$osudroid_source_path/.git" ]; then
    "$git_bin" clone https://github.com/osudroid/osu-droid.git "$root_dir/$osudroid_source_path"
  fi

  "$git_bin" -C "$root_dir/$osudroid_source_path" fetch origin master
  "$git_bin" -C "$root_dir/$osudroid_source_path" checkout master
  "$git_bin" -C "$root_dir/$osudroid_source_path" reset --hard origin/master
fi

language_pack_tag=$("$python_bin" - "$root_dir/$osudroid_source_path/build.gradle" <<'PY'
import re
import sys
from pathlib import Path

content = Path(sys.argv[1]).read_text()
match = re.search(r"com\.github\.osudroid:language-pack:([^']+)'", content)
if match is None:
    raise SystemExit("language-pack dependency not found")

print(match.group(1))
PY
)

if "$git_bin" -C "$root_dir" submodule status -- "$language_pack_path" >/dev/null 2>&1; then
  "$git_bin" -C "$root_dir" submodule sync --recursive "$language_pack_path"
  "$git_bin" -C "$root_dir" submodule update --init --recursive "$language_pack_path"
else
  if [ ! -e "$root_dir/$language_pack_path/.git" ]; then
    "$git_bin" clone https://github.com/osudroid/language-pack.git "$root_dir/$language_pack_path"
  fi
fi

"$git_bin" -C "$root_dir/$language_pack_path" fetch origin "refs/tags/$language_pack_tag:refs/tags/$language_pack_tag"
"$git_bin" -C "$root_dir/$language_pack_path" checkout "$language_pack_tag"

osudroid_source_commit=$("$git_bin" -C "$root_dir/$osudroid_source_path" rev-parse HEAD)
language_pack_commit=$("$git_bin" -C "$root_dir/$language_pack_path" rev-parse HEAD)

"$python_bin" - "$lock_file" "$osudroid_source_commit" "$language_pack_tag" "$language_pack_commit" <<'PY'
import json
import sys
from pathlib import Path

lock_path = Path(sys.argv[1])
osudroid_source_commit = sys.argv[2]
language_pack_tag = sys.argv[3]
language_pack_commit = sys.argv[4]
data = json.loads(lock_path.read_text())
data["sources"]["osu-droid-legacy"]["commit"] = osudroid_source_commit
data["sources"]["osu-droid-language-pack"]["tag"] = language_pack_tag
data["sources"]["osu-droid-language-pack"]["commit"] = language_pack_commit
lock_path.write_text(json.dumps(data, indent=2) + "\n")
PY

echo "osu!droid source synced to $osudroid_source_commit"
echo "osu-droid-language-pack synced to $language_pack_tag ($language_pack_commit)"
