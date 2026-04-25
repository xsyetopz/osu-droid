namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

public sealed class ReferenceEmptyHitWindow() : ReferenceHitWindow(null)
{
    public override double GreatWindow => 0.0;

    public override double OkWindow => 0.0;

    public override double MehWindow => 0.0;
}
