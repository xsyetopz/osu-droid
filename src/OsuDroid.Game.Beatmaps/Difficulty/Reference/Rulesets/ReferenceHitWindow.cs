namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

public abstract class ReferenceHitWindow(double? overallDifficulty = 5.0)
{
    public double OverallDifficulty { get; set; } = overallDifficulty ?? 5.0;

    public abstract double GreatWindow { get; }

    public abstract double OkWindow { get; }

    public abstract double MehWindow { get; }

    public const double MissWindow = 400.0;
}
