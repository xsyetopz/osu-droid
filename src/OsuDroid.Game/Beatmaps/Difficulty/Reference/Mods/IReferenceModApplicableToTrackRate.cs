namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public interface IReferenceModApplicableToTrackRate
{
    float ApplyToRate(double time, float rate = 1f);
}
