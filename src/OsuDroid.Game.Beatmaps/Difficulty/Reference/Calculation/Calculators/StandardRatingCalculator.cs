using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Calculators;

internal sealed class StandardRatingCalculator(
    IEnumerable<Mod> mods,
    int totalHits,
    double approachRate,
    double overallDifficulty,
    double mechanicalDifficultyRating,
    double sliderFactor
)
{
    public double ComputeAimRating(double aimDifficultyValue)
    {
        if (HasMod<ModAutopilot>())
        {
            return 0d;
        }

        double aimRating = CalculateDifficultyRating(aimDifficultyValue);
        if (HasMod<ModRelax>())
        {
            aimRating *= 0.9;
        }

        double ratingMultiplier = 1d;
        double approachRateLengthBonus =
            0.95
            + 0.4 * System.Math.Min(1d, totalHits / 2000d)
            + (totalHits > 2000 ? System.Math.Log10(totalHits / 2000d) * 0.5 : 0);
        double approachRateFactor =
            HasMod<ModRelax>() ? 0d
            : approachRate > 10.33 ? 0.3 * (approachRate - 10.33)
            : approachRate < 8 ? 0.05 * (approachRate - 8)
            : 0d;

        ratingMultiplier += approachRateLengthBonus * approachRateFactor;

        if (HasMod<ModHidden>())
        {
            double visibilityFactor = CalculateAimVisibilityFactor();
            ratingMultiplier += CalculateVisibilityBonus(
                mods,
                approachRate,
                visibilityFactor,
                sliderFactor
            );
        }

        ratingMultiplier *=
            0.98 + System.Math.Pow(System.Math.Max(0d, overallDifficulty), 2) / 2500;
        return aimRating * System.Math.Cbrt(ratingMultiplier);
    }

    public double ComputeSpeedRating(double speedDifficultyValue)
    {
        if (HasMod<ModRelax>())
        {
            return 0d;
        }

        double speedRating = CalculateDifficultyRating(speedDifficultyValue);
        if (HasMod<ModAutopilot>())
        {
            speedRating *= 0.5;
        }

        double ratingMultiplier = 1d;
        double approachRateLengthBonus =
            0.95
            + 0.4 * System.Math.Min(1d, totalHits / 2000d)
            + (totalHits > 2000 ? System.Math.Log10(totalHits / 2000d) * 0.5 : 0);
        double approachRateFactor =
            HasMod<ModAutopilot>() ? 0d
            : approachRate > 10.33 ? 0.3 * (approachRate - 10.33)
            : 0d;

        ratingMultiplier += approachRateLengthBonus * approachRateFactor;

        if (HasMod<ModHidden>())
        {
            double visibilityFactor = CalculateSpeedVisibilityFactor();
            ratingMultiplier += CalculateVisibilityBonus(
                mods,
                approachRate,
                visibilityFactor,
                sliderFactor
            );
        }

        ratingMultiplier *= 0.95 + System.Math.Pow(System.Math.Max(0d, overallDifficulty), 2) / 750;
        return speedRating * System.Math.Cbrt(ratingMultiplier);
    }

    public double ComputeFlashlightRating(double flashlightDifficultyValue)
    {
        if (!HasMod<ModFlashlight>())
        {
            return 0d;
        }

        double flashlightRating = CalculateDifficultyRating(flashlightDifficultyValue);
        if (HasMod<ModRelax>())
        {
            flashlightRating += 0.7;
        }
        else if (HasMod<ModAutopilot>())
        {
            flashlightRating *= 0.4;
        }

        double ratingMultiplier =
            0.7
            + 0.1 * System.Math.Min(1d, totalHits / 200d)
            + (totalHits > 200 ? 0.2 * System.Math.Min(1d, (totalHits - 200) / 200d) : 0d);
        ratingMultiplier *=
            0.98 + System.Math.Pow(System.Math.Max(0d, overallDifficulty), 2) / 2500;
        return flashlightRating * System.Math.Sqrt(ratingMultiplier);
    }

    private double CalculateAimVisibilityFactor()
    {
        const double endpoint = 11.5;
        double mechanicalDifficultyFactor = Interpolation.ReverseLinear(
            mechanicalDifficultyRating,
            5,
            10
        );
        double start = Interpolation.Linear(9d, 10.33, mechanicalDifficultyFactor);
        return Interpolation.ReverseLinear(approachRate, endpoint, start);
    }

    private double CalculateSpeedVisibilityFactor()
    {
        const double endpoint = 11.5;
        double mechanicalDifficultyFactor = Interpolation.ReverseLinear(
            mechanicalDifficultyRating,
            5,
            10
        );
        double start = Interpolation.Linear(10d, 10.33, mechanicalDifficultyFactor);
        return Interpolation.ReverseLinear(approachRate, endpoint, start);
    }

    private bool HasMod<TMod>()
        where TMod : Mod => mods.Any(static mod => mod is TMod);

    public static double CalculateDifficultyRating(double difficultyValue) =>
        System.Math.Sqrt(difficultyValue) * DifficultyMultiplier;

    public static double CalculateVisibilityBonus(
        IEnumerable<Mod> mods,
        double approachRate,
        double visibilityFactor = 1d,
        double sliderFactor = 1d
    )
    {
        ModHidden? hidden = mods.OfType<ModHidden>().FirstOrDefault();
        bool alwaysPartiallyVisible =
            hidden?.OnlyFadeApproachCircles ?? mods.Any(static mod => mod is ModTraceable);
        double readingBonus =
            (alwaysPartiallyVisible ? 0.025 : 0.04) * (12 - System.Math.Max(approachRate, 7));
        readingBonus *= visibilityFactor;

        double sliderVisibilityFactor = System.Math.Pow(sliderFactor, 3);
        if (approachRate < 7)
        {
            readingBonus +=
                (alwaysPartiallyVisible ? 0.02 : 0.045)
                * (7 - System.Math.Max(approachRate, 0))
                * sliderVisibilityFactor;
        }

        if (approachRate < 0)
        {
            readingBonus +=
                (alwaysPartiallyVisible ? 0.01 : 0.1)
                * (1 - System.Math.Pow(1.5, approachRate))
                * sliderVisibilityFactor;
        }

        return readingBonus;
    }

    private const double DifficultyMultiplier = 0.0675;
}
