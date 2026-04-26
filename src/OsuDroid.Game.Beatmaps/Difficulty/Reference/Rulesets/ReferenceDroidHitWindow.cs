namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

public sealed class ReferenceDroidHitWindow(double? overallDifficulty = 5.0)
    : ReferenceHitWindow(overallDifficulty)
{
    public override double GreatWindow => 75 + 5 * (5 - OverallDifficulty);

    public override double OkWindow => 150 + 10 * (5 - OverallDifficulty);

    public override double MehWindow => 250 + 10 * (5 - OverallDifficulty);

    public static double HitWindow300ToOverallDifficulty(double value) => 5 - (value - 75) / 5;

    public static double HitWindow100ToOverallDifficulty(double value) => 5 - (value - 150) / 10;

    public static double HitWindow50ToOverallDifficulty(double value) => 5 - (value - 250) / 10;
}
