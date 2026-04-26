using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public sealed class ReferenceModDifficultyAdjust(
    float? circleSize = null,
    float? approachRate = null,
    float? overallDifficulty = null,
    float? healthDrainRate = null
) : ReferenceMod, IReferenceModApplicableToDifficultyWithMods
{
    private float? _defaultCircleSize;
    private float? _defaultOverallDifficulty;

    public float? CircleSize { get; set; } = circleSize;

    public float? ApproachRate { get; set; } = approachRate;

    public float? OverallDifficulty { get; set; } = overallDifficulty;

    public float? HealthDrainRate { get; set; } = healthDrainRate;

    public override bool RequiresConfiguration => true;

    public override bool IsRelevant =>
        CircleSize.HasValue
        || ApproachRate.HasValue
        || OverallDifficulty.HasValue
        || HealthDrainRate.HasValue;

    public override float ScoreMultiplier
    {
        get
        {
            float multiplier = 1f;

            if (CircleSize is { } cs && _defaultCircleSize is { } defaultCs)
            {
                float diff = cs - defaultCs;
                multiplier *=
                    diff >= 0f
                        ? 1f + 0.0075f * (float)System.Math.Pow(diff, 1.5)
                        : 2f / (1f + (float)System.Math.Exp(-0.5f * diff));
            }

            if (OverallDifficulty is { } od && _defaultOverallDifficulty is { } defaultOd)
            {
                float diff = od - defaultOd;
                multiplier *=
                    diff >= 0f
                        ? 1f + 0.005f * (float)System.Math.Pow(diff, 1.3)
                        : 2f / (1f + (float)System.Math.Exp(-0.25f * diff));
            }

            return multiplier;
        }
    }

    public void ApplyDefaultValues(ReferenceBeatmapDifficulty difficulty)
    {
        _defaultCircleSize = difficulty.GameplayCircleSize;
        _defaultOverallDifficulty = difficulty.OverallDifficulty;
    }

    public void ApplyToDifficulty(
        GameMode mode,
        ReferenceBeatmapDifficulty difficulty,
        IEnumerable<ReferenceMod> mods
    )
    {
        difficulty.DifficultyCircleSize = CircleSize ?? difficulty.DifficultyCircleSize;
        difficulty.GameplayCircleSize = CircleSize ?? difficulty.GameplayCircleSize;
        difficulty.ApproachRate = ApproachRate ?? difficulty.ApproachRate;
        difficulty.OverallDifficulty = OverallDifficulty ?? difficulty.OverallDifficulty;
        difficulty.HealthDrainRate = HealthDrainRate ?? difficulty.HealthDrainRate;

        if (ApproachRate.HasValue && mods.Any(mod => mod is ReferenceModReplayV6))
        {
            double preempt = ReferenceBeatmapDifficulty.DifficultyRange(
                ApproachRate.Value,
                ReferenceDifficultyTiming.PreemptMax,
                ReferenceDifficultyTiming.PreemptMid,
                ReferenceDifficultyTiming.PreemptMin
            );
            float trackRate = ReferenceModApplicator.CalculateRateWithMods(
                mods,
                double.PositiveInfinity
            );

            difficulty.ApproachRate = (float)
                ReferenceBeatmapDifficulty.InverseDifficultyRange(
                    preempt * trackRate,
                    ReferenceDifficultyTiming.PreemptMax,
                    ReferenceDifficultyTiming.PreemptMid,
                    ReferenceDifficultyTiming.PreemptMin
                );
        }
    }
}
