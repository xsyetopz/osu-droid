namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public abstract class ReferenceModRateAdjust(float trackRateMultiplier = 1f)
    : ReferenceMod,
        IReferenceModApplicableToTrackRate
{
    public float TrackRateMultiplier { get; set; } = trackRateMultiplier;

    public override bool IsRelevant => TrackRateMultiplier != 1f;

    public override float ScoreMultiplier =>
        new ReferenceModRateAdjustHelper(TrackRateMultiplier).ScoreMultiplier;

    public float ApplyToRate(double time, float rate = 1f) => rate * TrackRateMultiplier;
}
