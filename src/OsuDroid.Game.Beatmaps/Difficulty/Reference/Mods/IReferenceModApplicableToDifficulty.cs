using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public interface IReferenceModApplicableToDifficulty
{
    void ApplyToDifficulty(
        GameMode mode,
        ReferenceBeatmapDifficulty difficulty,
        IEnumerable<IReferenceModFacilitatesAdjustment> adjustmentMods
    );
}
