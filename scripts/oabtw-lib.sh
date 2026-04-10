#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DOCS_ROOT="$REPO_ROOT/docs"
ORIGINAL_DOCS_DIR="$DOCS_ROOT/original-codebase"
ARCH_DOCS_DIR="$DOCS_ROOT/architecture"
SCRATCH_ROOT="$REPO_ROOT/.openagentsbtw/codex-analysis"
REWRITE_BRIEF="$DOCS_ROOT/libgdx_rewrite_brief.md"

OABTW_BIN_DIR="${OABTW_BIN_DIR:-$HOME/.codex/openagentsbtw/bin}"
OABTW_CODEX="${OABTW_CODEX:-$OABTW_BIN_DIR/oabtw-codex}"
OABTW_PEER="${OABTW_PEER:-$OABTW_BIN_DIR/oabtw-codex-peer}"

timestamp() {
    date +"%Y-%m-%dT%H-%M-%S"
}

slugify() {
    local input="${1:-default}"
    printf '%s' "$input" \
        | tr '[:upper:]' '[:lower:]' \
        | sed -E 's/[^a-z0-9]+/-/g; s/^-+//; s/-+$//'
}

ensure_layout() {
    mkdir -p "$ORIGINAL_DOCS_DIR" "$ARCH_DOCS_DIR" "$SCRATCH_ROOT"
}

require_command() {
    local path="$1"
    local name="$2"

    if [[ ! -x "$path" ]]; then
        printf 'Missing %s at %s\n' "$name" "$path" >&2
        exit 1
    fi
}

require_oabtw() {
    require_command "$OABTW_CODEX" "oabtw-codex"
}

make_run_dir() {
    local kind="$1"
    local scope="${2:-default}"
    local run_dir="$SCRATCH_ROOT/$(timestamp)-$(slugify "$kind")-$(slugify "$scope")"

    mkdir -p "$run_dir/results" "$run_dir/handoffs"
    printf '%s\n' "$run_dir"
}

write_run_brief() {
    local output_file="$1"
    local kind="$2"
    local scope="$3"

    cat > "$output_file" <<EOF
# openagentsbtw $kind run

- Repo: $REPO_ROOT
- Scope: $scope
- Run root: $(dirname "$output_file")

## Mandatory context
- Preserve gameplay behavior, compatibility semantics, and import expectations.
- Do not preserve AndEngine structure or Android-specific app architecture.
- The rewrite target is a fresh libGDX project that follows official libGDX Android+iOS guidance.
- Prefer evidence with exact files, symbols, and boundaries.

## Existing rewrite brief
EOF

    if [[ -f "$REWRITE_BRIEF" ]]; then
        printf '\n```\n' >> "$output_file"
        cat "$REWRITE_BRIEF" >> "$output_file"
        printf '\n```\n' >> "$output_file"
    else
        printf '\nRewrite brief not found at %s\n' "$REWRITE_BRIEF" >> "$output_file"
    fi
}

run_codox_job() {
    local mode="$1"
    local output_file="$2"
    local prompt="$3"

    "$OABTW_CODEX" "$mode" "$prompt" > "$output_file"
}

copy_if_exists() {
    local source_file="$1"
    local target_file="$2"

    if [[ -f "$source_file" ]]; then
        cp "$source_file" "$target_file"
    fi
}
