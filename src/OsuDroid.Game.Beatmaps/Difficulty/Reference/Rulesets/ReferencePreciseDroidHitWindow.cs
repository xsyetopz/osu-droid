namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

public sealed class ReferencePreciseDroidHitWindow(double? overallDifficulty = 5.0) : ReferenceHitWindow(overallDifficulty)
{
    public override double GreatWindow => 55 + 6 * (5 - OverallDifficulty);

    public override double OkWindow => 120 + 8 * (5 - OverallDifficulty);

    public override double MehWindow => 180 + 10 * (5 - OverallDifficulty);

    public static double HitWindow300ToOverallDifficulty(double value) => 5 - (value - 55) / 6;

    public static double HitWindow100ToOverallDifficulty(double value) => 5 - (value - 120) / 8;

    public static double HitWindow50ToOverallDifficulty(double value) => 5 - (value - 180) / 10;
}
