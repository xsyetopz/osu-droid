using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;

internal static class StandardSpeedEvaluator
{
    private static readonly double SingleSpacingThreshold =
        DifficultyHitObject.NormalizedDiameter * 1.25;
    private const double MinSpeedBonus = 75;
    private const double DistanceMultiplier = 0.8;

    public static double EvaluateDifficultyOf(
        StandardDifficultyHitObject current,
        IEnumerable<Mod> mods
    )
    {
        if (current.Obj is Spinner)
        {
            return 0d;
        }

        var prev = current.Previous(0) as StandardDifficultyHitObject;
        double strainTime = current.StrainTime;
        double doubletapness = 1 - current.GetDoubletapness(current.Next(0));
        strainTime /= System.Math.Clamp(strainTime / current.FullGreatWindow / 0.93, 0.92, 1);

        double speedBonus = 0d;
        if (strainTime < MinSpeedBonus)
        {
            speedBonus = 0.75 * System.Math.Pow((MinSpeedBonus - strainTime) / 40d, 2);
        }

        double travelDistance = prev?.TravelDistance ?? 0d;
        double distance = System.Math.Min(
            SingleSpacingThreshold,
            travelDistance + current.MinimumJumpDistance
        );
        double distanceBonus = mods.Any(static mod => mod is ModAutopilot)
            ? 0d
            : System.Math.Pow(distance / SingleSpacingThreshold, 3.95) * DistanceMultiplier;

        distanceBonus *= System.Math.Sqrt(current.SmallCircleBonus);
        double difficulty = (1 + speedBonus + distanceBonus) * 1000 / strainTime;
        return difficulty * doubletapness;
    }
}
