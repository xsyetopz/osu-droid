using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Utils;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;

internal static class StandardAimEvaluator
{
    private const double WideAngleMultiplier = 1.5;
    private const double AcuteAngleMultiplier = 2.55;
    private const double SliderMultiplier = 1.35;
    private const double VelocityChangeMultiplier = 0.75;
    private const double WiggleMultiplier = 1.02;

    public static double EvaluateDifficultyOf(StandardDifficultyHitObject current, bool withSliders)
    {
        if (current.Obj is Spinner || current.Index <= 1 || current.Previous(0)?.Obj is Spinner)
        {
            return 0d;
        }

        var last = (StandardDifficultyHitObject)current.Previous(0)!;
        var lastLast = (StandardDifficultyHitObject)current.Previous(1)!;
        var last2 = current.Previous(2) as StandardDifficultyHitObject;

        float radius = DifficultyHitObject.NormalizedRadius;
        float diameter = DifficultyHitObject.NormalizedDiameter;

        double currentVelocity = current.LazyJumpDistance / current.StrainTime;
        if (last.Obj is Slider && withSliders)
        {
            double travelVelocity = last.TravelDistance / last.TravelTime;
            double movementVelocity = current.MinimumJumpDistance / current.MinimumJumpTime;
            currentVelocity = System.Math.Max(currentVelocity, movementVelocity + travelVelocity);
        }

        double prevVelocity = last.LazyJumpDistance / last.StrainTime;
        if (lastLast.Obj is Slider && withSliders)
        {
            double travelVelocity = lastLast.TravelDistance / lastLast.TravelTime;
            double movementVelocity = last.MinimumJumpDistance / last.MinimumJumpTime;
            prevVelocity = System.Math.Max(prevVelocity, movementVelocity + travelVelocity);
        }

        double wideAngleBonus = 0d;
        double acuteAngleBonus = 0d;
        double sliderBonus = 0d;
        double velocityChangeBonus = 0d;
        double wiggleBonus = 0d;
        double strain = currentVelocity;

        if (current.Angle.HasValue && last.Angle.HasValue)
        {
            double currentAngle = current.Angle.Value;
            double lastAngle = last.Angle.Value;
            double angleBonus = System.Math.Min(currentVelocity, prevVelocity);

            if (System.Math.Max(current.StrainTime, last.StrainTime) < 1.25 * System.Math.Min(current.StrainTime, last.StrainTime))
            {
                acuteAngleBonus = CalculateAcuteAngleBonus(currentAngle);
                acuteAngleBonus *= 0.08 + 0.92 * (1 - System.Math.Min(acuteAngleBonus, System.Math.Pow(CalculateAcuteAngleBonus(lastAngle), 3)));
                acuteAngleBonus *=
                    angleBonus *
                    DifficultyCalculationUtils.Smootherstep(DifficultyCalculationUtils.MillisecondsToBpm(current.StrainTime, 2), 300, 400) *
                    DifficultyCalculationUtils.Smootherstep(current.LazyJumpDistance, diameter, diameter * 2d);
            }

            wideAngleBonus = CalculateWideAngleBonus(currentAngle);
            wideAngleBonus *= 1 - System.Math.Min(wideAngleBonus, System.Math.Pow(CalculateWideAngleBonus(lastAngle), 3));
            wideAngleBonus *= angleBonus * DifficultyCalculationUtils.Smootherstep(current.LazyJumpDistance, 0, diameter);

            wiggleBonus =
                angleBonus *
                DifficultyCalculationUtils.Smootherstep(current.LazyJumpDistance, radius, diameter) *
                System.Math.Pow(Interpolation.ReverseLinear(current.LazyJumpDistance, diameter * 3d, diameter), 1.8) *
                DifficultyCalculationUtils.Smootherstep(currentAngle, DegreesToRadians(110), DegreesToRadians(60)) *
                DifficultyCalculationUtils.Smootherstep(last.LazyJumpDistance, radius, diameter) *
                System.Math.Pow(Interpolation.ReverseLinear(last.LazyJumpDistance, diameter * 3d, diameter), 1.8) *
                DifficultyCalculationUtils.Smootherstep(lastAngle, DegreesToRadians(110), DegreesToRadians(60));

            if (last2 is not null)
            {
                float distanceSquared = last2.Obj.DifficultyStackedPosition.DistanceSquaredTo(last.Obj.DifficultyStackedPosition);
                if (distanceSquared < 1f)
                {
                    double distance = System.Math.Sqrt(distanceSquared);
                    wideAngleBonus *= 1 - 0.35 * (1 - distance);
                }
            }
        }

        if (System.Math.Max(prevVelocity, currentVelocity) != 0d)
        {
            prevVelocity = (last.LazyJumpDistance + lastLast.TravelDistance) / last.StrainTime;
            currentVelocity = (current.LazyJumpDistance + last.TravelDistance) / current.StrainTime;

            double distanceRatio = DifficultyCalculationUtils.Smoothstep(
                System.Math.Abs(prevVelocity - currentVelocity) / System.Math.Max(prevVelocity, currentVelocity),
                0,
                1);

            double overlapVelocityBuff = System.Math.Min(125 / System.Math.Min(current.StrainTime, last.StrainTime), System.Math.Abs(prevVelocity - currentVelocity));
            velocityChangeBonus = overlapVelocityBuff * distanceRatio;
            velocityChangeBonus *= System.Math.Pow(System.Math.Min(current.StrainTime, last.StrainTime) / System.Math.Max(current.StrainTime, last.StrainTime), 2);
        }

        if (last.Obj is Slider)
        {
            sliderBonus = last.TravelDistance / last.TravelTime;
        }

        strain += wiggleBonus * WiggleMultiplier;
        strain += velocityChangeBonus * VelocityChangeMultiplier;
        strain += System.Math.Max(acuteAngleBonus * AcuteAngleMultiplier, wideAngleBonus * WideAngleMultiplier);
        strain *= current.SmallCircleBonus;

        if (withSliders)
        {
            strain += sliderBonus * SliderMultiplier;
        }

        return strain;
    }

    private static double CalculateWideAngleBonus(double angle) =>
        DifficultyCalculationUtils.Smoothstep(angle, DegreesToRadians(40), DegreesToRadians(140));

    private static double CalculateAcuteAngleBonus(double angle) =>
        DifficultyCalculationUtils.Smoothstep(angle, DegreesToRadians(140), DegreesToRadians(40));

    private static double DegreesToRadians(double angle) => angle * System.Math.PI / 180d;
}
