using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Utils;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;

internal static class DroidReadingEvaluator
{
    private static readonly IReadOnlyList<Mod> EmptyMods = [];
    private const double ReadingWindowSize = 3000d;
    private static readonly double DistanceInfluenceThreshold =
        DifficultyHitObject.NormalizedDiameter * 1.25;
    private const double HiddenMultiplier = 0.5;
    private const double DensityMultiplier = 0.8;
    private const double DensityDifficultyBase = 1.5;
    private const double PreemptBalancingFactor = 220000d;
    private const int PreemptStartingPoint = 475;

    public static double EvaluateDifficultyOf(
        DroidDifficultyHitObject current,
        double clockRate,
        IEnumerable<Mod> mods
    )
    {
        if (current.Obj is Spinner || current.IsOverlapping(true) || current.Index <= 0)
        {
            return 0d;
        }

        double constantAngleNerfFactor = GetConstantAngleNerfFactor(current);
        double velocityFactor = System.Math.Max(
            1d,
            current.MinimumJumpDistance / current.StrainTime
        );
        double pastObjectDifficultyInfluence = 0d;

        foreach (DroidDifficultyHitObject prev in RetrievePastVisibleObjects(current))
        {
            double prevDifficulty = current.OpacityAt(prev.Obj.StartTime, EmptyMods);
            prevDifficulty *= DifficultyCalculationUtils.Smootherstep(
                prev.LazyJumpDistance,
                15,
                DistanceInfluenceThreshold
            );
            prevDifficulty *= GetTimeNerfFactor(current.StartTime - prev.StartTime);
            pastObjectDifficultyInfluence += prevDifficulty;
        }

        double noteDensityDifficulty =
            System.Math.Pow(pastObjectDifficultyInfluence, 1.45)
            * 0.9
            * constantAngleNerfFactor
            * velocityFactor;
        noteDensityDifficulty = System.Math.Max(0d, noteDensityDifficulty - DensityDifficultyBase);
        noteDensityDifficulty = System.Math.Pow(noteDensityDifficulty, 0.8) * DensityMultiplier;

        double hiddenDifficulty = 0d;
        if (mods.Any(static mod => mod is ModHidden))
        {
            double timeSpentInvisible = GetDurationSpentInvisible(current) / clockRate;
            double timeSpentInvisibleFactor = System.Math.Pow(timeSpentInvisible, 2.1) * 0.0001;
            double futureObjectDifficultyInfluence = CalculateCurrentVisibleObjectsDensity(current);
            double densityFactor =
                System.Math.Pow(
                    System.Math.Max(
                        1d,
                        futureObjectDifficultyInfluence + pastObjectDifficultyInfluence - 2
                    ),
                    2.3
                ) * 3.2;

            hiddenDifficulty +=
                (timeSpentInvisibleFactor + densityFactor)
                * constantAngleNerfFactor
                * velocityFactor
                * 0.007;
            hiddenDifficulty = System.Math.Pow(hiddenDifficulty, 0.85) * HiddenMultiplier;

            var prev = (DroidDifficultyHitObject)current.Previous(0)!;
            if (
                current.LazyJumpDistance == 0d
                && current.OpacityAt(prev.Obj.StartTime + prev.Obj.TimePreempt, mods) == 0d
                && prev.StartTime + prev.TimePreempt > current.StartTime
            )
            {
                hiddenDifficulty +=
                    (HiddenMultiplier * 1303) / System.Math.Pow(current.StrainTime, 1.5);
            }
        }

        double preemptDifficulty =
            System.Math.Pow(
                (
                    PreemptStartingPoint
                    - current.TimePreempt
                    + System.Math.Abs(current.TimePreempt - PreemptStartingPoint)
                ) / 2d,
                2.35
            )
            / PreemptBalancingFactor
            * constantAngleNerfFactor
            * velocityFactor;

        double sliderDifficulty = 0d;
        if (current.Obj is Slider slider)
        {
            double scalingFactor = 50d / slider.DifficultyRadius;
            double pixelTravelDistance = current.LazyTravelDistance / scalingFactor;
            double currentVelocity = pixelTravelDistance / current.TravelTime;
            double spanTravelDistance = pixelTravelDistance / slider.SpanCount;

            sliderDifficulty +=
                System.Math.Min(4d, currentVelocity * 0.8) * (spanTravelDistance / 125d);
            double cumulativeStrainTime = 0d;

            for (int i = 0; i < System.Math.Min(current.Index, 4); ++i)
            {
                var last = current.Previous(i) as DroidDifficultyHitObject;
                if (last is null)
                {
                    break;
                }

                cumulativeStrainTime += last.StrainTime;
                if (last.Obj is not Slider lastSlider || last.IsOverlapping(true))
                {
                    continue;
                }

                double lastPixelTravelDistance = last.LazyTravelDistance / scalingFactor;
                double lastVelocity = lastPixelTravelDistance / last.TravelTime;
                double lastSpanTravelDistance = lastPixelTravelDistance / lastSlider.SpanCount;

                sliderDifficulty +=
                    System.Math.Min(4d, 0.8 * System.Math.Abs(currentVelocity - lastVelocity))
                    * (lastSpanTravelDistance / 150d)
                    * System.Math.Min(1d, 250d / cumulativeStrainTime);
            }
        }

        return noteDensityDifficulty + hiddenDifficulty + preemptDifficulty + sliderDifficulty;
    }

    private static IEnumerable<DroidDifficultyHitObject> RetrievePastVisibleObjects(
        DroidDifficultyHitObject current
    )
    {
        for (int i = 0; i < current.Index; ++i)
        {
            var prev = current.Previous(i) as DroidDifficultyHitObject;
            if (prev is null)
            {
                yield break;
            }

            if (
                current.StartTime - prev.StartTime > ReadingWindowSize
                || prev.StartTime + prev.TimePreempt < current.StartTime
            )
            {
                yield break;
            }

            if (prev.IsOverlapping(true))
            {
                continue;
            }

            yield return prev;
        }
    }

    private static double CalculateCurrentVisibleObjectsDensity(DroidDifficultyHitObject current)
    {
        double visibleObjectCount = 0d;
        var next = current.Next(0) as DroidDifficultyHitObject;

        while (next is not null)
        {
            double timeDifference = next.StartTime - current.StartTime;
            if (
                timeDifference > ReadingWindowSize
                || current.StartTime + current.TimePreempt < next.StartTime
            )
            {
                break;
            }

            if (next.IsOverlapping(true))
            {
                next = next.Next(0) as DroidDifficultyHitObject;
                continue;
            }

            double timeNerfFactor = GetTimeNerfFactor(timeDifference);
            visibleObjectCount += next.OpacityAt(current.Obj.StartTime, EmptyMods) * timeNerfFactor;
            next = next.Next(0) as DroidDifficultyHitObject;
        }

        return visibleObjectCount;
    }

    private static double GetDurationSpentInvisible(DroidDifficultyHitObject current)
    {
        HitObject obj = current.Obj;
        double fadeOutStartTime = obj.StartTime - obj.TimePreempt + obj.TimeFadeIn;
        double fadeOutDuration = obj.TimePreempt * ModHidden.FadeOutDurationMultiplier;
        return fadeOutStartTime + fadeOutDuration - (obj.StartTime - obj.TimePreempt);
    }

    private static double GetConstantAngleNerfFactor(DroidDifficultyHitObject current)
    {
        const double maxTimeLimit = 2000d;
        const double minTimeLimit = 200d;

        double constantAngleCount = 0d;
        int index = 0;
        double currentTimeGap = 0d;

        while (currentTimeGap < maxTimeLimit)
        {
            DifficultyHitObject? loopObj = current.Previous(index);
            if (loopObj is null)
            {
                break;
            }

            if (loopObj.Angle.HasValue && current.Angle.HasValue)
            {
                double angleDifference = System.Math.Abs(current.Angle.Value - loopObj.Angle.Value);
                double longIntervalFactor = System.Math.Clamp(
                    1 - (loopObj.StrainTime - minTimeLimit) / (maxTimeLimit - minTimeLimit),
                    0d,
                    1d
                );
                constantAngleCount +=
                    System.Math.Cos(3 * System.Math.Min(System.Math.PI / 6, angleDifference))
                    * longIntervalFactor;
            }

            currentTimeGap = current.StartTime - loopObj.StartTime;
            ++index;
        }

        return System.Math.Clamp(2 / constantAngleCount, 0.2, 1);
    }

    private static double GetTimeNerfFactor(double deltaTime) =>
        System.Math.Clamp(2 - deltaTime / (ReadingWindowSize / 2d), 0d, 1d);
}
