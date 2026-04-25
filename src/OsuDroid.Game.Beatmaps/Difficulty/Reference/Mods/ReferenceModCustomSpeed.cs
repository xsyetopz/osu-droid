namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public sealed class ReferenceModCustomSpeed(float trackRateMultiplier = 1f) : ReferenceModRateAdjust(trackRateMultiplier)
{
    public override bool RequiresConfiguration => true;
}
