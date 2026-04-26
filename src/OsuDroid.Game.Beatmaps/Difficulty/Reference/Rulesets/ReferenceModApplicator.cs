using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

public static class ReferenceModApplicator
{
    public static float CalculateRateWithMods(IEnumerable<ReferenceMod> mods, double time = 0.0)
    {
        float rate = 1f;

        foreach (var mod in mods)
        {
            if (mod is IReferenceModApplicableToTrackRate rateMod)
            {
                rate = rateMod.ApplyToRate(time, rate);
            }
        }

        return rate;
    }

    public static void ApplyModsToBeatmapDifficulty(
        ReferenceBeatmapDifficulty difficulty,
        GameMode mode,
        IEnumerable<ReferenceMod> mods,
        bool withRateChange = false
    )
    {
        var materializedMods = mods as IReadOnlyList<ReferenceMod> ?? mods.ToArray();
        var adjustmentMods = materializedMods
            .OfType<IReferenceModFacilitatesAdjustment>()
            .ToArray();

        foreach (var mod in materializedMods.OfType<IReferenceModApplicableToDifficulty>())
        {
            mod.ApplyToDifficulty(mode, difficulty, adjustmentMods);
        }

        foreach (var mod in materializedMods.OfType<IReferenceModApplicableToDifficultyWithMods>())
        {
            mod.ApplyToDifficulty(mode, difficulty, materializedMods);
        }

        if (!withRateChange)
        {
            return;
        }

        float trackRate = CalculateRateWithMods(materializedMods, double.PositiveInfinity);

        double preempt =
            ReferenceBeatmapDifficulty.DifficultyRange(
                difficulty.ApproachRate,
                ReferenceDifficultyTiming.PreemptMax,
                ReferenceDifficultyTiming.PreemptMid,
                ReferenceDifficultyTiming.PreemptMin
            ) / trackRate;

        difficulty.ApproachRate = (float)
            ReferenceBeatmapDifficulty.InverseDifficultyRange(
                preempt,
                ReferenceDifficultyTiming.PreemptMax,
                ReferenceDifficultyTiming.PreemptMid,
                ReferenceDifficultyTiming.PreemptMin
            );

        bool isPrecise = materializedMods.Any(mod => mod is ReferenceModPrecise);
        ReferenceHitWindow hitWindow = isPrecise
            ? new ReferencePreciseDroidHitWindow(difficulty.OverallDifficulty)
            : new ReferenceDroidHitWindow(difficulty.OverallDifficulty);
        double greatWindow = hitWindow.GreatWindow / trackRate;

        difficulty.OverallDifficulty = (float)(
            isPrecise
                ? ReferencePreciseDroidHitWindow.HitWindow300ToOverallDifficulty(greatWindow)
                : ReferenceDroidHitWindow.HitWindow300ToOverallDifficulty(greatWindow)
        );
    }
}
