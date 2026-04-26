using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Utils;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;

internal static class DroidRhythmEvaluator
{
    private const int HistoryTimeMax = 5000;
    private const int HistoryObjectsMax = 32;
    private const double RhythmOverallMultiplier = 0.95;
    private const double RhythmRatioMultiplier = 15d;

    public static double EvaluateDifficultyOf(
        DroidDifficultyHitObject current,
        bool useSliderAccuracy
    )
    {
        if (current.Obj is Spinner)
        {
            return 1d;
        }

        double deltaDifferenceEpsilon = current.FullGreatWindow * 0.3;
        double rhythmComplexitySum = 0d;

        Island island = new(deltaDifferenceEpsilon);
        Island previousIsland = new(deltaDifferenceEpsilon);
        Dictionary<Island, int> islandCounts = [];

        double startRatio = 0d;
        bool firstDeltaSwitch = false;
        int rhythmStart = 0;

        int historicalNoteCount = System.Math.Min(current.Index, HistoryObjectsMax);
        List<DroidDifficultyHitObject> validPrevious = [];

        for (int i = 0; i < historicalNoteCount; ++i)
        {
            var prev = current.Previous(i) as DroidDifficultyHitObject;
            if (prev is null)
            {
                break;
            }

            if (!prev.IsOverlapping(false))
            {
                validPrevious.Add(prev);
            }
        }

        if (validPrevious.Count < 3)
        {
            return 1d;
        }

        while (
            rhythmStart < validPrevious.Count - 2
            && current.StartTime - validPrevious[rhythmStart].StartTime < HistoryTimeMax
        )
        {
            ++rhythmStart;
        }

        DroidDifficultyHitObject prevObject = validPrevious[rhythmStart];
        DroidDifficultyHitObject lastObject = validPrevious[rhythmStart + 1];

        for (int i = rhythmStart; i >= 1; --i)
        {
            DroidDifficultyHitObject currentObject = validPrevious[i - 1];

            double timeDecay =
                (HistoryTimeMax - (current.StartTime - currentObject.StartTime)) / HistoryTimeMax;
            double noteDecay = (validPrevious.Count - i) / (double)validPrevious.Count;
            double currentHistoricalDecay = System.Math.Min(noteDecay, timeDecay);

            double currentDelta = System.Math.Max(1e-7, currentObject.DeltaTime);
            double prevDelta = System.Math.Max(1e-7, prevObject.DeltaTime);
            double lastDelta = System.Math.Max(1e-7, lastObject.DeltaTime);

            double deltaDifference =
                System.Math.Max(prevDelta, currentDelta) / System.Math.Min(prevDelta, currentDelta);
            double deltaDifferenceFraction =
                deltaDifference - System.Math.Truncate(deltaDifference);
            double currentRatio =
                1
                + RhythmRatioMultiplier
                    * System.Math.Min(
                        0.5,
                        DifficultyCalculationUtils.SmoothstepBellCurve(deltaDifferenceFraction)
                    );
            double differenceMultiplier = System.Math.Clamp(2 - deltaDifference / 8, 0, 1);
            double windowPenalty = System.Math.Clamp(
                (System.Math.Abs(prevDelta - currentDelta) - deltaDifferenceEpsilon)
                    / deltaDifferenceEpsilon,
                0,
                1
            );
            double effectiveRatio = windowPenalty * currentRatio * differenceMultiplier;

            if (firstDeltaSwitch)
            {
                if (System.Math.Abs(prevDelta - currentDelta) < deltaDifferenceEpsilon)
                {
                    island.AddDelta((int)currentDelta);
                }
                else
                {
                    if (!useSliderAccuracy)
                    {
                        if (currentObject.Obj is Slider)
                        {
                            effectiveRatio /= 8;
                        }

                        if (prevObject.Obj is Slider)
                        {
                            effectiveRatio *= 0.3;
                        }
                    }

                    if (island.IsSimilarPolarity(previousIsland))
                    {
                        effectiveRatio /= 2;
                    }

                    if (
                        lastDelta > prevDelta + deltaDifferenceEpsilon
                        && prevDelta > currentDelta + deltaDifferenceEpsilon
                    )
                    {
                        effectiveRatio /= 8;
                    }

                    if (previousIsland.DeltaCount == island.DeltaCount)
                    {
                        effectiveRatio /= 2;
                    }

                    bool islandFound = false;
                    foreach ((Island otherIsland, int count) in islandCounts.ToArray())
                    {
                        if (!island.Equals(otherIsland))
                        {
                            continue;
                        }

                        islandFound = true;
                        int islandCount = count;
                        if (previousIsland.Equals(island))
                        {
                            islandCounts[otherIsland] = ++islandCount;
                        }

                        effectiveRatio *= System.Math.Min(
                            3d / islandCount,
                            System.Math.Pow(
                                1d / islandCount,
                                DifficultyCalculationUtils.Logistic(island.Delta, 58.33, 0.24, 2.75)
                            )
                        );
                        break;
                    }

                    if (!islandFound)
                    {
                        islandCounts[island] = 1;
                    }

                    effectiveRatio *= 1 - prevObject.GetDoubletapness(prevObject.Next(0)) * 0.75;
                    rhythmComplexitySum +=
                        System.Math.Sqrt(effectiveRatio * startRatio) * currentHistoricalDecay;
                    startRatio = effectiveRatio;
                    previousIsland = island;

                    if (prevDelta + deltaDifferenceEpsilon < currentDelta)
                    {
                        firstDeltaSwitch = false;
                    }

                    island = new Island((int)currentDelta, deltaDifferenceEpsilon);
                }
            }
            else if (prevDelta > currentDelta + deltaDifferenceEpsilon)
            {
                firstDeltaSwitch = true;
                if (currentObject.Obj is Slider)
                {
                    effectiveRatio *= 0.6;
                }

                if (prevObject.Obj is Slider)
                {
                    effectiveRatio *= 0.6;
                }

                startRatio = effectiveRatio;
                island = new Island((int)currentDelta, deltaDifferenceEpsilon);
            }

            lastObject = prevObject;
            prevObject = currentObject;
        }

        return System.Math.Sqrt(4 + rhythmComplexitySum * RhythmOverallMultiplier) / 2;
    }
}
