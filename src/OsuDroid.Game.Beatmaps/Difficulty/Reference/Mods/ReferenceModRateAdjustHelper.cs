namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public sealed class ReferenceModRateAdjustHelper(float trackRateMultiplier)
{
    public float ScoreMultiplier =>
        trackRateMultiplier > 1f
            ? 1f + (trackRateMultiplier - 1f) * 0.24f
            : (float)System.Math.Pow(0.3f, (1f - trackRateMultiplier) * 4f);
}
