namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

public sealed class ReferenceStandardHitWindow(double? overallDifficulty = 5.0) : ReferenceHitWindow(overallDifficulty)
{
    public override double GreatWindow => System.Math.Floor(80 - 6 * OverallDifficulty) - 0.5;

    public override double OkWindow => System.Math.Floor(140 - 8 * OverallDifficulty) - 0.5;

    public override double MehWindow => System.Math.Floor(200 - 10 * OverallDifficulty) - 0.5;
}
