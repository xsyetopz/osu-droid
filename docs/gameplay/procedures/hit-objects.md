# Procedure: Hit Objects

## Circle

```text
procedure update_circle(circle, song_time, cursors):
  visible_start = circle.start_time - circle.time_preempt
  miss_deadline = circle.start_time + 0.400

  if song_time < visible_start:
    keep hidden
  else:
    fade_progress = clamp((song_time - visible_start) / circle.time_fade_in, 0, 1)
    approach_progress = clamp((circle.start_time - song_time) / circle.time_preempt, 0, 1)
    set circle alpha from fade_progress
    set approach circle scale from approach_progress

  event = earliest unprocessed cursor down event that:
    event.time >= circle.start_time - hittable_range
    squared_distance(event.position, circle.position) <= circle.screen_radius^2

  if event exists:
    accuracy = abs(event.hit_time_seconds - circle.start_time)
    if forced_result is MISS or accuracy > hitWindow.mehWindow / 1000:
      notify circle hit with score 0
    else if forced_result is HIT300 or accuracy <= hitWindow.greatWindow / 1000:
      notify circle hit with score 300
    else if forced_result is HIT100 or accuracy <= hitWindow.okWindow / 1000:
      notify circle hit with score 100
    else:
      notify circle hit with score 50
    play hit samples
    mark finished
    return

  if song_time > miss_deadline:
    notify circle miss
    mark finished
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayHitCircle.java:221-297`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameObject.java:137-213`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2268-2321`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2300-2315`

## Slider

```text
procedure update_slider(slider, song_time, cursors):
  if head has not been judged and cursor hit passes circle hit-test:
    notify SLIDER_START with score 30 when combo increment is allowed
    start slider tracking

  current_path_position = curve_position(song_time, repeat_index)
  holding = any cursor is pressed inside follow-circle radius at current_path_position
  set follow-circle visible when holding

  for each pending tick before song_time:
    if holding:
      notify SLIDER_TICK with score 10
    else:
      notify nested miss with score -1

  for each pending repeat before song_time:
    if holding and cursor is near repeat position:
      notify SLIDER_REPEAT with score 30
    else:
      notify nested miss with score -1

  if song_time reaches tail:
    if slider final result is 0:
      notify whole-slider miss with score 0
    else:
      notify SLIDER_END with final score 50, 100, or 300
    mark finished
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySlider.java:803-908`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySlider.java:911-1288`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2327-2411`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2350-2405`

## Spinner

```text
procedure update_spinner(spinner, song_time, cursors):
  if spinner is active:
    for each cursor movement around spinner center:
      add signed angular delta to rotation accumulator
    update RPM display and clear-progress display
    if fullRotations >= needRotations:
      mark clear state
    if rotations exceed bonus threshold after clear:
      notify spinner bonus with score 1000
    play spinner loop samples according to spin state

  if song_time reaches spinner end:
    percentFilled = (abs(rotations) + fullRotations) / needRotations
    if percentFilled > 0.9:
      score = 50
    if percentFilled > 0.95:
      score = 100
    if clear:
      score = 300
    if replay result exists:
      score = replay accuracy modulo 4 mapped to 0, 50, 100, or 300
    stop spinner samples
    notify spinner hit with score and bonus count
    mark finished
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplaySpinner.java:131-255`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:137-324`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2421-2475`
