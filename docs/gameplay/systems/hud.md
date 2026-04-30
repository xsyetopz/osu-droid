# HUD System

## Default layout

- Default HUD contains accuracy, leaderboard, combo, pie song progress, health bar, score, back button, unstable rate, average offset, and hit error meter. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/HUDSkinData.kt:27-96`)
- Default layout moves accuracy below score and pie progress beside accuracy after all elements are attached. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/GameplayHUD.kt:188-219`)
- `GameScene.start()` removes `HUDLeaderboard` only when `Config.isShowScoreboard()` is false and not in HUD editor mode. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1356-1358`)

## Event fan-out

- HUD forwards gameplay update, touch down, hit object lifetime start, note hit, break state change, and accuracy register to every HUD element and editor selector. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/GameplayHUD.kt:261-305`)
- HUD detaches itself when current UI scene is no longer the gameplay scene. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/GameplayHUD.kt:72-79`)

## Hold back button

- Back button always shows, size `72`, initial alpha `0.25`, and contains front circle, progress circle, and back arrow. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:16-31`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:73-80`, `third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:135-137`)
- Hold fraction sets progress circle portion, scales circles/arrow by `1 + progress / 2`, and interpolates alpha from `0.25` to `0.5`. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:54-67`)
- Press increments hold time; reaching required time calls `gameScene.pause()`. Release decays hold time at twice elapsed time; moving outside cancels press. (`third_party/osu-droid-legacy/src/com/osudroid/ui/v2/hud/elements/HUDBackButton.kt:83-131`)
- Default required hold is `300ms` from `back_button_press_time`; full values table is `docs/gameplay/contracts/ui-animation-values.md`. (`third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/Config.java:223-236`)
