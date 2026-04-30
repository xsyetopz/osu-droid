# Song Select to Gameplay

## Mod button

- Song menu mods button switches to `selection-mods-over` on press, records press origin, and opens ModMenu only on release without scroll-distance cancel. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:420-445`)
- Beatmap information and score attribute surfaces read `ModMenu.enabledMods` for current mod-adjusted values. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:922-935`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:1768-1776`)

## Start request

- SongMenu cancels calculation jobs, handles multiplayer room beatmap selection separately, stops song menu music, closes ModMenu, dismisses search, and calls `game.startGame(beatmapInfo, null, ModMenu.INSTANCE.getEnabledMods())`. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:1095-1114`)
- `GameScene.startGame` deep-copies supplied mods for normal play, forces Autoplay only for HUD editor mode, and reuses last mods on restart. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:960-975`)
- Loader receives the selected mods and shows them in `ModsIndicator`. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:24-35`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/GameLoaderScene.kt:125-128`)

## Multiplayer branch

- Room scene opens ModMenu from its mods action and starts gameplay with `ModMenu.enabledMods`. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/multi/RoomScene.kt:370-370`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/multi/RoomScene.kt:972-972`)
- ModMenu close sends room mods when host, player mods when non-host and updates are allowed, or clears waiting state when updates are skipped. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:431-447`)
