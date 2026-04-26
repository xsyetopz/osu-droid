namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

public sealed class ReferenceUnadjustedStandardHitWindow(double? overallDifficulty = 5.0)
    : ReferenceHitWindow(overallDifficulty)
{
    public override double GreatWindow => 80 - 6 * OverallDifficulty;

    public override double OkWindow => 140 - 8 * OverallDifficulty;

    public override double MehWindow => 200 - 10 * OverallDifficulty;

    public static double HitWindow300ToOverallDifficulty(double value) => (80 - value) / 6;

    public static double HitWindow100ToOverallDifficulty(double value) => (140 - value) / 8;

    public static double HitWindow50ToOverallDifficulty(double value) => (200 - value) / 10;
}
