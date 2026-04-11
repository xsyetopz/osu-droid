# Rewrite Architecture

## Source split
- `osu-framework` supplies runtime and mobile platform integration.
- `ppy/osu` supplies screen behavior, route structure, and localisation wording.
- `osu-droid-legacy` supplies mobile layout cues only.

## Product boundaries
- Android and iOS only.
- No desktop launcher.

## First milestone
- Guest-first main menu
- Optional login overlay
- Local song select
- Separate online browse surface
