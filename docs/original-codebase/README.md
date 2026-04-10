# Legacy Analysis Workspace

This directory stores durable local notes about the existing Android/AndEngine codebase.

## Files
- `overview.md`: cross-subsystem exploration synthesis
- `gameplay-rules.md`: gameplay, beatmap, difficulty, replay, and math findings
- `runtime-shell.md`: launcher, scene, UI, AndEngine, and audio/runtime findings
- `services-data.md`: data, multiplayer, beatmap services, and Android-facing integration findings
- `*-review.md`: risk-focused review outputs for the same lanes
- `cross-boundary-review.md`: cross-lane review of hidden coupling and rewrite blockers

## Required sections
- `Current Shape`
- `Rewrite-Relevant Behavior`
- `Architecture To Discard`
- `Hotspots`
- `Missing Tests`
- `Next Traces`

These notes are intentionally ignored by Git so they can evolve quickly during long-running analysis.
