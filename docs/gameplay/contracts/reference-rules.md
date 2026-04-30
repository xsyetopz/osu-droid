# Reference Rules

## Source order

- Behavior source: `third_party/osu-droid-legacy/src`. Code behavior outranks platform convention.
- Visual source: Android screenshots plus Android resource and skin asset usage. Use screenshots after source behavior is mapped.
- Runtime assets in the rewrite must be owned by this repo; `third_party/osu-droid-legacy` remains reference input only.

## Citation rule

- Every behavior statement in this pack uses exact source path plus line or range.
- Code changes that alter behavior must update cited reference notes.
- If a cited line and screenshot disagree, record both as `Source behavior` and `Screenshot evidence`.

## Porting rule

- Keep source ordering when it changes behavior: loader before start, HUD on overlay, pause as overlay, nested slider judgement before tail result.
- Keep config defaults unless source reads another value from beatmap, room, replay, or per-beatmap options.
- Multiplayer branches count as behavior even when local implementation starts with solo gameplay.
