# Scoring Values

## HP and combo mutation

| Input score | `k` flag | HP delta | Combo | Counters / score | Source |
| --- | --- | ---: | --- | --- | --- |
| `1000` | any | no direct HP | unchanged | add bonus score only | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:159-177` |
| `1..49` | any | `+0.05` | `+1` | add non-combo score | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:167-177` |
| `0` | `true` | `-(5 + HPDrain) / 100` | reset to `0` | no miss counter in this branch | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:178-185` |
| `300` | `true` | `+0.10` | `+1` when allowed | `hit300++`, `hit300k++` when `g` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:187-198` |
| `300` | `false` | `+0.05` | `+1` when allowed | `hit300++` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:187-198` |
| `100` | `true` | `+0.15` | `+1` when allowed | `hit100++`, `hit100k++` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:199-209` |
| `100` | `false` | `+0.05` | `+1` when allowed | `hit100++` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:199-209` |
| `50` | any | `+0.05` | `+1` when allowed | `hit50++` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:210-217` |
| default miss | any | `-(5 + HPDrain) / 100` | reset to `0` | `misses++`, add `0` for ScoreV2 refresh | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:218-228` |

HP clamps to `[0, 1]`; HP reaching `0` consumes life and can set `isAlive=false` when failure is enabled. Source: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:128-146`

## Score formulas

```text
if ScoreV2:
  if amount == 1000:
    bonusScore += amount
  currentMaxCombo = scoreMaxCombo
  if combo_score and currentCombo == currentMaxCombo:
    currentMaxCombo += 1
  comboPortion = 0.7 * currentMaxCombo / beatmapMaxCombo
  accuracyPortion = 0.3 * accuracy^10 * notesHit / beatmapNoteCount
  totalScore = int(1_000_000 * (comboPortion + accuracyPortion)) + bonusScore
else:
  add amount
  if combo_score:
    add int(amount * currentCombo * diffModifier / 25)
```

Constants: `scoreV2MaxScore=1000000`, `scoreV2AccPortion=0.3`, `scoreV2ComboPortion=0.7`. Sources: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:32-34`, `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:241-281`

## Accuracy and rank

| Value | Formula / rule | Source |
| --- | --- | --- |
| Accuracy | `(hit300 * 6 + hit100 * 2 + hit50) / (6 * notesHit)` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:231-239` |
| No hits accuracy | `1` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:231-239` |
| `XH` / `X` | no 100, no 50, no misses; Hidden or Flashlight gives `XH` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:283-315` |
| `SH` / `S` | `hit300 / notesHit > 0.9`, no misses, `hit50 / notesHit < 0.01`; Hidden or Flashlight gives `SH` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:283-315` |
| `A` | `hit300 / notesHit > 0.8` with no misses, or `> 0.9` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:283-315` |
| `B` | `hit300 / notesHit > 0.7` with no misses, or `> 0.8` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:283-315` |
| `C` | `hit300 / notesHit > 0.6` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:283-315` |
| `D` | fallback | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/scoring/StatisticV2.java:283-315` |

## Slider nested score values

| Event | Score value | Statistic side effect | Source |
| --- | ---: | --- | --- |
| Slider start | `30` | `addSliderHeadHit()` when combo increment allowed | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2366-2379` |
| Slider repeat | `30` | `addSliderRepeatHit()` when combo increment allowed | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2381-2387` |
| Slider tick | `10` | `addSliderTickHit()` when combo increment allowed | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2389-2395` |
| Slider end | final slider result | `addSliderEndHit()` when combo increment allowed | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2397-2405` |

Nested slider miss uses `score=-1`: plays combobreak when combo `> 30`, applies SuddenDeath failure, registers `0` with combo break, and returns. Source: `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2350-2364`

## Circle and spinner result thresholds

| Object | Threshold logic | Source |
| --- | --- | --- |
| Circle miss | `accuracy > mehWindow / 1000` or forced miss | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2300-2315` |
| Circle 300 | forced 300 or `accuracy <= greatWindow / 1000` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2306-2315` |
| Circle 100 | forced 100 or `accuracy <= okWindow / 1000` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2306-2315` |
| Circle 50 | non-miss fallback | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameScene.java:2306-2315` |
| Spinner 50 | filled percent `> 0.9` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:301-323` |
| Spinner 100 | filled percent `> 0.95` | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:301-323` |
| Spinner 300 | clear state reached | `third_party/osu-droid-legacy/src/ru/nsu/ccfit/zuev/osu/game/GameplayModernSpinner.java:301-323` |

