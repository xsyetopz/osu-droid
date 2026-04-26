using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;

internal static class StandardFlashlightEvaluator
{
    public static double EvaluateDifficultyOf(
        StandardDifficultyHitObject current,
        IEnumerable<Mod> mods
    )
    {
        if (current.Obj is Spinner)
        {
            return 0d;
        }

        double scalingFactor = 52d / current.Obj.DifficultyRadius;
        double smallDistNerf = 1d;
        double cumulativeStrainTime = 0d;
        double result = 0d;
        StandardDifficultyHitObject last = current;
        double angleRepeatCount = 0d;

        for (int i = 0; i < System.Math.Min(current.Index, 10); ++i)
        {
            var currentObject = (StandardDifficultyHitObject)current.Previous(i)!;
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
                    1 + 0.4 * (1 - current.OpacityAt(currentObject.Obj.StartTime, mods));
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
        if (mods.Any(static mod => mod is ModHidden))
        {
            result *= 1.2;
        }

        const double minAngleMultiplier = 0.2;
        result *= minAngleMultiplier + (1 - minAngleMultiplier) / (angleRepeatCount + 1);

        double sliderBonus = 0d;
        if (current.Obj is Slider slider)
        {
            double pixelTravelDistance = current.LazyTravelDistance / scalingFactor;
            sliderBonus = System.Math.Pow(
                System.Math.Max(0d, pixelTravelDistance / current.TravelTime - 0.5),
                0.5
            );
            sliderBonus *= pixelTravelDistance;
            sliderBonus /= slider.RepeatCount + 1;
        }

        result += sliderBonus * 1.3;
        return result;
    }
}
