using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;

internal static class DroidFlashlightEvaluator
{
    private const double MaxOpacityBonus = 0.4;
    private const double HiddenBonus = 0.2;
    private const double TraceableCircleBonus = 0.15;
    private const double TraceableObjectBonus = 0.1;
    private const double MinVelocity = 0.5;
    private const double SliderMultiplier = 1.3;
    private const double MinAngleMultiplier = 0.2;

    public static double EvaluateDifficultyOf(
        DroidDifficultyHitObject current,
        IEnumerable<Mod> mods,
        bool withSliders
    )
    {
        if (current.Obj is Spinner || current.IsOverlapping(true))
        {
            return 0d;
        }

        double scalingFactor = 52d / current.Obj.DifficultyRadius;
        double smallDistNerf = 1d;
        double cumulativeStrainTime = 0d;
        double result = 0d;
        DroidDifficultyHitObject last = current;
        double angleRepeatCount = 0d;

        for (int i = 0; i < System.Math.Min(current.Index, 10); ++i)
        {
            var currentObject = (DroidDifficultyHitObject)current.Previous(i)!;
            cumulativeStrainTime += last.StrainTime;

            if (currentObject.Obj is not Spinner)
            {
                double jumpDistance = current.Obj.DifficultyStackedPosition.DistanceTo(
                    currentObject.Obj.DifficultyStackedEndPosition
                );
                if (i == 0)
                {
                    smallDistNerf = System.Math.Min(1d, jumpDistance / 75d);
                }

                double stackNerf = System.Math.Min(
                    1d,
                    currentObject.LazyJumpDistance / scalingFactor / 25d
                );
                double opacityBonus =
                    1
                    + MaxOpacityBonus * (1 - current.OpacityAt(currentObject.Obj.StartTime, mods));
                result +=
                    stackNerf * opacityBonus * scalingFactor * jumpDistance / cumulativeStrainTime;

                if (
                    currentObject.Angle.HasValue
                    && current.Angle.HasValue
                    && System.Math.Abs(currentObject.Angle.Value - current.Angle.Value) < 0.02
                )
                {
                    angleRepeatCount += System.Math.Max(0d, 1 - 0.1 * i);
                }
            }

            last = currentObject;
        }

        result = System.Math.Pow(smallDistNerf * result, 2);

        if (mods.Any(static m => m is ModHidden))
        {
            result *= 1 + HiddenBonus;
        }
        else if (mods.Any(static m => m is ModTraceable))
        {
            result *= 1 + (current.Obj is HitCircle ? TraceableCircleBonus : TraceableObjectBonus);
        }

        result *= MinAngleMultiplier + (1 - MinAngleMultiplier) / (angleRepeatCount + 1);

        double sliderBonus = 0d;
        if (current.Obj is Slider slider && withSliders)
        {
            double pixelTravelDistance = current.LazyTravelDistance / scalingFactor;
            sliderBonus = System.Math.Pow(
                System.Math.Max(0d, pixelTravelDistance / current.TravelTime - MinVelocity),
                0.5
            );
            sliderBonus *= pixelTravelDistance;
            if (slider.RepeatCount > 0)
            {
                sliderBonus /= slider.RepeatCount + 1;
            }
        }

        result += sliderBonus * SliderMultiplier;
        return result;
    }
}
