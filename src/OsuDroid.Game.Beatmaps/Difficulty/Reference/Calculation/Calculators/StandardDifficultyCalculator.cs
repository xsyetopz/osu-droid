#pragma warning disable CA1822

using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Calculators;

internal sealed class StandardDifficultyCalculator
    : DifficultyCalculator<
        StandardPlayableBeatmap,
        StandardDifficultyHitObject,
        StandardDifficultyAttributes
    >
{
    private const double StarRatingMultiplier = 0.0265;
    private const double StandardPerformanceFinalMultiplier = 1.24;
    public const long Version = 1762003732001;

    protected override StandardDifficultyAttributes CreateDifficultyAttributes(
        PlayableBeatmap beatmap,
        Skill<StandardDifficultyHitObject>[] skills,
        StandardDifficultyHitObject[] objects,
        bool forReplay
    )
    {
        StandardDifficultyAttributes attributes = new()
        {
            Mods = beatmap.Mods.Values.ToHashSet(),
            ClockRate = beatmap.SpeedMultiplier,
            MaxCombo = beatmap.MaxCombo,
            HitCircleCount = beatmap.HitObjects.CircleCount,
            SliderCount = beatmap.HitObjects.SliderCount,
            SpinnerCount = beatmap.HitObjects.SpinnerCount,
            ApproachRate = beatmap.Difficulty.Ar,
            OverallDifficulty = beatmap.Difficulty.Od,
        };

        StandardAim? aim = FindSkill<StandardAim>(skills, skill => skill.WithSliders);
        StandardAim? aimNoSlider = FindSkill<StandardAim>(skills, skill => !skill.WithSliders);
        StandardSpeed? speed = FindSkill<StandardSpeed>(skills);
        StandardFlashlight? flashlight = FindSkill<StandardFlashlight>(skills);

        double aimDifficultyValue = aim?.DifficultyValue() ?? 0d;
        attributes.AimDifficultSliderCount = aim?.CountDifficultSliders() ?? 0d;
        attributes.AimDifficultStrainCount = aim?.CountTopWeightedStrains() ?? 0d;
        attributes.AimSliderFactor =
            aimDifficultyValue > 0
                ? StandardRatingCalculator.CalculateDifficultyRating(
                    aimNoSlider?.DifficultyValue() ?? 0d
                ) / StandardRatingCalculator.CalculateDifficultyRating(aimDifficultyValue)
                : 1;

        double aimNoSliderTopWeightedSliderCount = aimNoSlider?.CountTopWeightedSliders() ?? 0d;
        double aimNoSliderDifficultStrainCount = aimNoSlider?.CountTopWeightedStrains() ?? 0d;
        attributes.AimTopWeightedSliderFactor =
            aimNoSliderTopWeightedSliderCount
            / System.Math.Max(
                1d,
                aimNoSliderDifficultStrainCount - aimNoSliderTopWeightedSliderCount
            );

        double speedDifficultyValue = speed?.DifficultyValue() ?? 0d;
        attributes.SpeedNoteCount = speed?.RelevantNoteCount() ?? 0d;
        attributes.SpeedDifficultStrainCount = speed?.CountTopWeightedStrains() ?? 0d;
        double speedTopWeightedSliderCount = speed?.CountTopWeightedSliders() ?? 0d;
        attributes.SpeedTopWeightedSliderFactor =
            speedTopWeightedSliderCount
            / System.Math.Max(
                1d,
                attributes.SpeedDifficultStrainCount - speedTopWeightedSliderCount
            );

        double mechanicalDifficultyRating = CalculateMechanicalDifficultyRating(
            aimDifficultyValue,
            speedDifficultyValue
        );
        StandardRatingCalculator ratingCalculator = new(
            beatmap.Mods.Values,
            beatmap.HitObjects.Objects.Count,
            attributes.ApproachRate,
            attributes.OverallDifficulty,
            mechanicalDifficultyRating,
            attributes.AimSliderFactor
        );

        attributes.AimDifficulty = ratingCalculator.ComputeAimRating(aimDifficultyValue);
        attributes.SpeedDifficulty = ratingCalculator.ComputeSpeedRating(speedDifficultyValue);
        attributes.FlashlightDifficulty = ratingCalculator.ComputeFlashlightRating(
            flashlight?.DifficultyValue() ?? 0d
        );

        double baseAimPerformance =
            StrainSkill<StandardDifficultyHitObject>.DifficultyToPerformance(
                attributes.AimDifficulty
            );
        double baseSpeedPerformance =
            StrainSkill<StandardDifficultyHitObject>.DifficultyToPerformance(
                attributes.SpeedDifficulty
            );
        double baseFlashlightPerformance = StandardFlashlight.DifficultyToPerformance(
            attributes.FlashlightDifficulty
        );
        double basePerformance = System.Math.Pow(
            System.Math.Pow(baseAimPerformance, 1.1)
                + System.Math.Pow(baseSpeedPerformance, 1.1)
                + System.Math.Pow(baseFlashlightPerformance, 1.1),
            1 / 1.1
        );

        attributes.StarRating = CalculateStarRating(basePerformance);
        return attributes;
    }

    protected override Skill<StandardDifficultyHitObject>[] CreateSkills(
        StandardPlayableBeatmap beatmap,
        bool forReplay
    )
    {
        IReadOnlyCollection<Mod> mods = beatmap.Mods.Values;
        List<Skill<StandardDifficultyHitObject>> skills = [];

        if (!beatmap.Mods.Contains(typeof(ModAutopilot)))
        {
            skills.Add(new StandardAim(mods, true));
            skills.Add(new StandardAim(mods, false));
        }

        if (!beatmap.Mods.Contains(typeof(ModRelax)))
        {
            skills.Add(new StandardSpeed(mods));
        }

        if (beatmap.Mods.Contains(typeof(ModFlashlight)))
        {
            skills.Add(new StandardFlashlight(mods));
        }

        return [.. skills];
    }

    protected override StandardDifficultyHitObject[] CreateDifficultyHitObjects(
        StandardPlayableBeatmap beatmap
    )
    {
        if (beatmap.HitObjects.Objects.Count == 0)
        {
            return [];
        }

        double clockRate = beatmap.SpeedMultiplier;
        var objects = beatmap.HitObjects.Objects;
        var result = new StandardDifficultyHitObject[objects.Count - 1];

        for (int i = 1; i < objects.Count; ++i)
        {
            result[i - 1] = new StandardDifficultyHitObject(
                objects[i],
                objects[i - 1],
                clockRate,
                result,
                i - 1
            );
            result[i - 1].ComputeProperties(clockRate);
        }

        return result;
    }

    protected override StandardPlayableBeatmap CreatePlayableBeatmap(
        Beatmap beatmap,
        IEnumerable<Mod>? mods
    ) => beatmap.CreateStandardPlayableBeatmap(mods);

    private double CalculateMechanicalDifficultyRating(
        double aimDifficultyValue,
        double speedDifficultyValue
    )
    {
        double aimValue = StrainSkill<StandardDifficultyHitObject>.DifficultyToPerformance(
            StandardRatingCalculator.CalculateDifficultyRating(aimDifficultyValue)
        );
        double speedValue = StrainSkill<StandardDifficultyHitObject>.DifficultyToPerformance(
            StandardRatingCalculator.CalculateDifficultyRating(speedDifficultyValue)
        );
        double totalValue = System.Math.Pow(
            System.Math.Pow(aimValue, 1.1) + System.Math.Pow(speedValue, 1.1),
            1 / 1.1
        );
        return CalculateStarRating(totalValue);
    }

    private static double CalculateStarRating(double basePerformance) =>
        basePerformance > 1e-5
            ? System.Math.Cbrt(StandardPerformanceFinalMultiplier)
                * StarRatingMultiplier
                * (System.Math.Cbrt(100000 / System.Math.Pow(2, 1 / 1.1) * basePerformance) + 4)
            : 0d;

    public static double CalculateRateAdjustedApproachRate(double approachRate, double clockRate)
    {
        double preempt =
            ReferenceBeatmapDifficulty.DifficultyRange(
                approachRate,
                HitObject.PreemptMax,
                HitObject.PreemptMid,
                HitObject.PreemptMin
            ) / clockRate;

        return ReferenceBeatmapDifficulty.InverseDifficultyRange(
            preempt,
            HitObject.PreemptMax,
            HitObject.PreemptMid,
            HitObject.PreemptMin
        );
    }

    public static double CalculateRateAdjustedOverallDifficulty(
        double overallDifficulty,
        double clockRate
    )
    {
        ReferenceStandardHitWindow hitWindow = new(overallDifficulty);
        double greatWindow = hitWindow.GreatWindow / clockRate;
        return (79.5 - greatWindow) / 6;
    }
}
