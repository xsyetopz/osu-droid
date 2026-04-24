namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

public sealed class ReferenceBeatmapDifficulty(float circleSize = 5f, float? approachRate = null, float overallDifficulty = 5f, float healthDrainRate = 5f)
{
    private float _approachRate = approachRate ?? float.NaN;

    public float DifficultyCircleSize { get; set; } = circleSize;

    public float GameplayCircleSize { get; set; } = circleSize;

    public float ApproachRate
    {
        get => float.IsNaN(_approachRate) ? OverallDifficulty : _approachRate;
        set => _approachRate = value;
    }

    public float OverallDifficulty { get; set; } = overallDifficulty;

    public float HealthDrainRate { get; set; } = healthDrainRate;

    public float Ar => ApproachRate;

    public float Od => OverallDifficulty;

    public double SliderMultiplier { get; set; } = 1.0;

    public double SliderTickRate { get; set; } = 1.0;

    public static double DifficultyRange(double difficulty, double min, double mid, double max)
    {
        if (difficulty > 5)
        {
            return mid + (max - mid) * (difficulty - 5) / 5;
        }

        return difficulty < 5 ? mid + (mid - min) * (difficulty - 5) / 5 : mid;
    }

    public static double InverseDifficultyRange(double difficultyValue, double diff0, double diff5, double diff10)
    {
        return System.Math.Sign(difficultyValue - diff5) == System.Math.Sign(diff10 - diff0)
            ? (difficultyValue - diff5) / (diff10 - diff5) * 5 + 5
            : (difficultyValue - diff5) / (diff5 - diff0) * 5 + 5;
    }
}
