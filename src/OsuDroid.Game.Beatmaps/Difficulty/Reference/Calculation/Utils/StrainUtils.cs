namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Utils;

internal static class StrainUtils
{
    public static double CountTopWeightedSliders(
        IReadOnlyList<double> sliderStrains,
        double difficultyValue
    )
    {
        if (sliderStrains.Count == 0)
        {
            return 0d;
        }

        double consistentTopStrain = difficultyValue / 10d;
        return consistentTopStrain == 0d
            ? 0d
            : sliderStrains.Sum(strain =>
                DifficultyCalculationUtils.Logistic(strain / consistentTopStrain, 0.88, 10, 1.1)
            );
    }
}
