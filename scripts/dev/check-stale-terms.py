#!/usr/bin/env python3
from __future__ import annotations

import pathlib
import re
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parents[2]
STALE_WORDS = ("TO" + "DO", "FIX" + "ME", "HA" + "CK", "tempor" + "ary", "compatibility " + "shim")
STALE_PATTERN = re.compile(r"\b(" + "|".join(re.escape(word) for word in STALE_WORDS) + r")\b", re.IGNORECASE)
REFERENCE_PATH_TOKEN = "osu-droid-" + "leg" + "acy"
REFERENCE_NAME_TOKEN = "leg" + "acy"


def tracked_files() -> list[pathlib.Path]:
    result = subprocess.run(
        ["git", "ls-files", "AGENTS.md", "src", "tests", "docs", "scripts"],
        cwd=ROOT,
        check=True,
        capture_output=True,
        text=True,
    )
    return [ROOT / line for line in result.stdout.splitlines() if line]


def is_reference_path_line(line: str) -> bool:
    return REFERENCE_PATH_TOKEN in line


def main() -> int:
    errors: list[str] = []
    for path in tracked_files():
        if not path.is_file() or {"bin", "obj"}.intersection(path.parts):
            continue
        try:
            text = path.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            continue
        for number, line in enumerate(text.splitlines(), start=1):
            if STALE_PATTERN.search(line):
                errors.append(f"{path.relative_to(ROOT)}:{number}: forbidden stale term: {line.strip()}")
            if REFERENCE_NAME_TOKEN in line.lower() and not is_reference_path_line(line):
                errors.append(f"{path.relative_to(ROOT)}:{number}: reference naming must stay path/provenance-only: {line.strip()}")
    if errors:
        print("Stale-term check failed:", file=sys.stderr)
        for error in errors:
            print(f"- {error}", file=sys.stderr)
        return 1
    print("Stale-term check passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
