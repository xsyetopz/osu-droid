using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public sealed class ModRelax : ReferenceMod;

public sealed class ModAutopilot : ReferenceMod;

public sealed class ModEasy : ReferenceMod, IReferenceModApplicableToDifficulty
{
    public void ApplyToDifficulty(
        GameMode mode,
        ReferenceBeatmapDifficulty difficulty,
        IEnumerable<IReferenceModFacilitatesAdjustment> adjustmentMods
    )
    {
        difficulty.DifficultyCircleSize *= 0.5f;
        difficulty.GameplayCircleSize *= 0.5f;
        difficulty.ApproachRate *= 0.5f;
        difficulty.OverallDifficulty *= 0.5f;
        difficulty.HealthDrainRate *= 0.5f;
    }
}

public sealed class ModReallyEasy : ReferenceMod;

public sealed class ModMirror : ReferenceMod;

public sealed class ModHardRock : ReferenceMod, IReferenceModApplicableToDifficulty
{
    public void ApplyToDifficulty(
        GameMode mode,
        ReferenceBeatmapDifficulty difficulty,
        IEnumerable<IReferenceModFacilitatesAdjustment> adjustmentMods
    )
    {
        difficulty.DifficultyCircleSize = System.Math.Min(
            difficulty.DifficultyCircleSize * 1.3f,
            10f
        );
        difficulty.GameplayCircleSize = System.Math.Min(difficulty.GameplayCircleSize * 1.3f, 10f);
        difficulty.ApproachRate = System.Math.Min(difficulty.ApproachRate * 1.4f, 10f);
        difficulty.OverallDifficulty = System.Math.Min(difficulty.OverallDifficulty * 1.4f, 10f);
        difficulty.HealthDrainRate = System.Math.Min(difficulty.HealthDrainRate * 1.4f, 10f);
    }
}

public sealed class ModHidden : ReferenceMod
{
    public const double FadeOutDurationMultiplier = 0.4;

    public bool OnlyFadeApproachCircles { get; init; }
}

public sealed class ModFlashlight : ReferenceMod;

public sealed class ModDifficultyAdjust : ReferenceMod, IReferenceModApplicableToDifficultyWithMods
{
    public void ApplyToDifficulty(
        GameMode mode,
        ReferenceBeatmapDifficulty difficulty,
        IEnumerable<ReferenceMod> mods
    )
    { }
}

public sealed class ModRateAdjust(float trackRateMultiplier = 1f)
    : ReferenceModRateAdjust(trackRateMultiplier);

public sealed class ModTimeRamp : ReferenceMod;

public sealed class ModTraceable : ReferenceMod;

public sealed class ModPrecise : ReferenceMod;

public sealed class ModScoreV2 : ReferenceMod;

public sealed class ModFreezeFrame : ReferenceMod;

public sealed class ModReplayV6 : ReferenceMod, IReferenceModFacilitatesAdjustment;
