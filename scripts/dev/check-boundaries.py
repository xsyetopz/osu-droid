#!/usr/bin/env python3
from __future__ import annotations

import pathlib
import re
import sys
import xml.etree.ElementTree as ET

ROOT = pathlib.Path(__file__).resolve().parents[2]

PROJECTS = {
    "OsuDroid.Game.UI": {"allowed": set[str]()},
    "OsuDroid.Game.Runtime": {"allowed": set[str]()},
    "OsuDroid.Game.Beatmaps": {"allowed": {"OsuDroid.Game.Runtime"}},
    "OsuDroid.Game.Compatibility": {"allowed": {"OsuDroid.Game.Beatmaps", "OsuDroid.Game.Runtime"}},
    "OsuDroid.Game": {
        "allowed": {
            "OsuDroid.Game.Beatmaps",
            "OsuDroid.Game.Compatibility",
            "OsuDroid.Game.Runtime",
            "OsuDroid.Game.UI",
        },
    },
    "OsuDroid.App": {"allowed": {"OsuDroid.Game", "OsuDroid.Game.Runtime", "OsuDroid.Game.UI"}},
}

NAMESPACE_PROJECTS = {
    "OsuDroid.Game.UI": "OsuDroid.Game.UI",
    "OsuDroid.Game.Runtime": "OsuDroid.Game.Runtime",
    "OsuDroid.Game.Beatmaps": "OsuDroid.Game.Beatmaps",
    "OsuDroid.Game.Compatibility": "OsuDroid.Game.Compatibility",
    "OsuDroid.Game": "OsuDroid.Game",
    "OsuDroid.App": "OsuDroid.App",
}


def project_name(path: pathlib.Path) -> str:
    tree = ET.parse(path)
    root_namespace = tree.find("./PropertyGroup/RootNamespace")
    if root_namespace is not None and root_namespace.text:
        return root_namespace.text.strip()
    return path.stem


def referenced_project_name(project_path: pathlib.Path, include: str) -> str:
    reference_path = (project_path.parent / pathlib.Path(include.replace("\\", "/"))).resolve()
    return project_name(reference_path)


def check_project_references() -> list[str]:
    errors: list[str] = []
    for project_path in sorted((ROOT / "src").glob("*/**/*.csproj")):
        owner = project_name(project_path)
        if owner not in PROJECTS:
            continue
        allowed = PROJECTS[owner]["allowed"]
        tree = ET.parse(project_path)
        for reference in tree.findall(".//ProjectReference"):
            include = reference.attrib.get("Include")
            if not include:
                continue
            target = referenced_project_name(project_path, include)
            if target not in allowed:
                errors.append(f"{project_path.relative_to(ROOT)} references {target}, but {owner} allows {sorted(allowed)}")
    return errors


def namespace_owner(namespace: str) -> str | None:
    matches = [project for prefix, project in NAMESPACE_PROJECTS.items() if namespace == prefix or namespace.startswith(prefix + ".")]
    return max(matches, key=len) if matches else None


def check_source_usings() -> list[str]:
    errors: list[str] = []
    for source in sorted((ROOT / "src").rglob("*.cs")):
        if {"bin", "obj"}.intersection(source.parts):
            continue
        owner = next((project for project in PROJECTS if project in source.parts), None)
        if owner is None:
            continue
        allowed = PROJECTS[owner]["allowed"] | {owner}
        text = source.read_text(encoding="utf-8")
        for match in re.finditer(r"^\s*using\s+(OsuDroid\.[\w.]+)\s*;", text, re.MULTILINE):
            imported_owner = namespace_owner(match.group(1))
            if imported_owner is not None and imported_owner not in allowed:
                errors.append(
                    f"{source.relative_to(ROOT)} imports {match.group(1)}, but {owner} allows {sorted(allowed)}"
                )
    return errors


def main() -> int:
    errors = check_project_references() + check_source_usings()
    if errors:
        print("Boundary check failed:", file=sys.stderr)
        for error in errors:
            print(f"- {error}", file=sys.stderr)
        return 1
    print("Boundary check passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
