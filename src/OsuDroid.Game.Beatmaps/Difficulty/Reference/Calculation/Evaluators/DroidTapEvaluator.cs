using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;

internal static class DroidTapEvaluator
{
    private const double MinSpeedBonus = 75;

    public static double EvaluateDifficultyOf(
        DroidDifficultyHitObject current,
        bool considerCheesability,
        double? strainTimeCap = null)
    {
        if (current.Obj is Spinner || current.IsOverlapping(false))
        {
            return 0d;
        }

        double doubletapness = considerCheesability ? 1 - current.GetDoubletapness(current.Next(0)) : 1d;
        double strainTime = strainTimeCap.HasValue
            ? System.Math.Max(50d, System.Math.Max(strainTimeCap.Value, current.StrainTime))
            : current.StrainTime;

        double speedBonus = 1d;
        if (current.StrainTime < MinSpeedBonus)
        {
            speedBonus += 0.75 * System.Math.Pow(ErfFast((MinSpeedBonus - strainTime) / 40d), 2);
        }

        return speedBonus * System.Math.Pow(doubletapness, 1.5) * 1000d / strainTime;
    }

    private static double ErfFast(double x)
    {
        double sign = System.Math.Sign(x);
        double a = System.Math.Abs(x);
        double t = 1 / (1 + 0.3275911 * a);
        double y = 1 - (((((1.061405429 * t - 1.453152027) * t + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t) * System.Math.Exp(-a * a);
        return sign * y;
    }
}
