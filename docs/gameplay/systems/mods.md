# Mods System

## State owner

- `ModMenu.enabledMods` is the selected mod container used by UI preview and gameplay start. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:42-68`)
- `ModsIndicator` displays mod icons only; it filters non-user-playable mods unless configured otherwise. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/ModsIndicator.kt:8-67`)
- `ModIcon` fetches `mod.iconTextureName`; missing textures fall back to acronym text in a rounded accent box. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModIcon.kt:35-78`)

## Menu behavior

- `ModMenu.show()` attaches ModMenu as child scene, hides presets in multiplayer, updates visible/enabled states, and parses selected beatmap. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:404-425`)
- `addMod` and `removeMod` update `enabledMods`, toggle selected state, notify customization menu, reset incompatible removed mod instances, and queue mod changes. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:594-639`)
- Queued changes are processed on managed update: ranked badge, incompatibility state, score multiplier, customization enablement, rate-mod music effects, beatmap parse, selected mods indicator, and presets update. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:541-592`)

## Difficulty preview

- `parseBeatmap` cancels prior calculation, loads selected beatmap for configured difficulty algorithm, lets original-beatmap-dependent mods read beatmap data, applies difficulty mods, updates AR/OD/CS/HP/BPM/multiplier badges, computes star rating, and updates song menu display. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:329-398`)

## Multiplayer rules

- `setMods` preserves allowed free-mod selections, rejects invalid multiplayer mods, handles host/player ownership, converts DoubleTime/NightCore by preference for non-hosts, and enforces ScoreV2 from room mods. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:454-499`)
- `clear` preserves non-host room-owned mods in free-mod rooms and preserves ScoreV2 in multiplayer. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:515-539`)
