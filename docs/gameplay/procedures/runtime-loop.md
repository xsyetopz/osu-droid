# Procedure: Runtime Loop

Use this order when porting. Later subsystems depend on earlier timing state.

```text
procedure gameplay_update(delta):
  if game is not started:
    update passive scene work
    return

  read music position and frame offset settings
  update replay/autoplay/autopilot cursor state
  update active timing point
  update storyboard and video jobs
  evaluate break window
  if in break:
    update break animator, HP recovery, and HUD break state
  else:
    hide break animator when leaving break

  update countdown state
  spawn objects whose preempt window has opened
  update active objects with cursor state
  remove finished objects
  process metronome samples
  update HUD, leaderboard, skip button, and fail/end state
  if beatmap ended or player failed:
    route to result/end path
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1394-1724`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1729-1845`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1848-1973`

```text
procedure update_passive(delta):
  update scene actions and loader-only animation
  do not advance object scoring
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:1990-1999`

```text
procedure skip_intro(force):
  if not force and elapsedTime > skipTime - 1:
    return
  play menuhit sample
  difference = skipTime - elapsedTime
  elapsedTime = skipTime
  elapsedTimeMs = ceil(elapsedTime * 1000)
  musicSeekTime = max(0, int(elapsedTimeMs - totalOffset * rate_at(elapsedTimeMs) * 1000))
  videoSeekTime = max(0, int(elapsedTimeMs - videoOffset * 1000))
  update passive objects by difference
  if elapsedTime >= rate_adjusted_offset and music is not started:
    start song service and set BGM volume
  seek song service to musicSeekTime
  if video is enabled:
    seek video to videoSeekTime
  remove skip button
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2018-2064`
