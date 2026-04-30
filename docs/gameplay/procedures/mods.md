# Procedure: Mods Handoff

```text
procedure open_mod_menu(song_menu_state):
  attach ModMenu as child scene
  hide presets when multiplayer rules require it
  copy current selected mods into menu state
  parse beatmap for preview stats
  update ranked badge, multiplier, and selected mod indicator
```

Sources: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:404-425`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:329-398`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:541-592`

```text
procedure toggle_mod(mod):
  if mod is already enabled:
    remove from enabled mods
    reset selected UI state
  else:
    add to enabled mods
    remove incompatible mods as required
    set selected UI state
  queue mod-change processing
```

Sources: `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:594-639`

```text
procedure start_game_from_song_menu():
  read selected beatmap and enabled mods
  if multiplayer room is active:
    apply room mod ownership and free-mod restrictions
  hand selected mods to GameScene.startGame
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:420-445`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:922-935`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/menu/SongMenu.java:1095-1114`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/modmenu/ModMenu.kt:454-499`

