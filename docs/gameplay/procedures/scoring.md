# Procedure: Scoring and Feedback

```text
procedure apply_object_result(object_result):
  if object_result.score == 1000:
    add bonus score only
    return

  if 0 < object_result.score < 50:
    hp += 0.05
    add raw non-combo score
    combo += 1
    return

  if object_result.score == 0 and object_result.k_flag:
    hp -= (5 + healthDrain) / 100
    maxCombo = max(maxCombo, combo)
    combo = 0
    return

  if object_result.score == 300:
    hp += 0.10 when k_flag else 0.05
    hit300 += 1
    if g_flag: hit300k += 1
    add score 300 with combo bonus
    if increment_combo: combo += 1

  if object_result.score == 100:
    hp += 0.15 when k_flag else 0.05
    hit100 += 1
    if k_flag: hit100k += 1
    add score 100 with combo bonus
    if increment_combo: combo += 1

  if object_result.score == 50:
    hp += 0.05
    hit50 += 1
    add score 50 with combo bonus
    if increment_combo: combo += 1

  otherwise:
    hp -= (5 + healthDrain) / 100
    misses += 1
    maxCombo = max(maxCombo, combo)
    combo = 0
    add score 0 for ScoreV2 refresh

  hp = clamp(hp, 0, 1)
  register accuracy result separately from score
  enqueue HUD events for score, combo, accuracy, and HP
  play configured hit samples
  if object ends a combo:
    apply combo-end visual state
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:128-228`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2175-2265`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2268-2491`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:3035-3046`

```text
procedure add_score(amount, combo_score):
  if ScoreV2 is active:
    if amount == 1000:
      bonusScore += amount
    currentMaxCombo = scoreMaxCombo
    if combo_score and currentCombo == currentMaxCombo:
      currentMaxCombo += 1
    comboPortion = 0.7 * currentMaxCombo / beatmapMaxCombo
    accuracyPortion = 0.3 * accuracy^10 * notesHit / beatmapNoteCount
    totalScore = int(1_000_000 * (comboPortion + accuracyPortion)) + bonusScore
  else if amount + amount * currentCombo * diffModifier / 25 > 0:
    totalScore += amount
    if combo_score:
      totalScore += int(amount * currentCombo * diffModifier / 25)
  scoreHash = high16Bits(totalScore)
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:32-34`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:241-281`

```text
procedure play_samples(object_sample_set):
  read object normal/addition sample sets
  resolve sample bank and volume
  play each requested hit sound once for the judged object event
```

Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2477-2491`
