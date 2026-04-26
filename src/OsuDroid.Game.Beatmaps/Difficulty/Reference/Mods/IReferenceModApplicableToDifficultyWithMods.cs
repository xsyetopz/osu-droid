using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public interface IReferenceModApplicableToDifficultyWithMods
{
    void ApplyToDifficulty(
        GameMode mode,
        ReferenceBeatmapDifficulty difficulty,
        IEnumerable<ReferenceMod> mods
    );
}
