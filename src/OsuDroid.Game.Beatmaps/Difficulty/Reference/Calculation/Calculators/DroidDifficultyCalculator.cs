#pragma warning disable CA1859

using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Calculators;

internal sealed class DroidDifficultyCalculator : DifficultyCalculator<DroidPlayableBeatmap, DroidDifficultyHitObject, DroidDifficultyAttributes>
{
    protected override HashSet<Type> DifficultyAdjustmentMods =>
        [.. base.DifficultyAdjustmentMods, typeof(ModPrecise), typeof(ModScoreV2), typeof(ModFreezeFrame), typeof(ModReplayV6)];

    private const int MinimumSectionObjectCount = 5;
    private const double ThreeFingerStrainThreshold = 175d;
    private const double DifficultyMultiplier = 0.18;
    public const long Version = 1759210780001;

    protected override DroidDifficultyAttributes CreateDifficultyAttributes(
        PlayableBeatmap beatmap,
        Skill<DroidDifficultyHitObject>[] skills,
        DroidDifficultyHitObject[] objects,
        bool forReplay)
    {
        DroidDifficultyAttributes attributes = new()
        {
            Mods = beatmap.Mods.Values.ToHashSet(),
            ClockRate = beatmap.SpeedMultiplier,
            MaxCombo = beatmap.MaxCombo,
            HitCircleCount = beatmap.HitObjects.CircleCount,
            SliderCount = beatmap.HitObjects.SliderCount,
            SpinnerCount = beatmap.HitObjects.SpinnerCount,
            OverallDifficulty = beatmap.Difficulty.Od,
        };

        PopulateAimAttributes(attributes, skills, forReplay);
        PopulateTapAttributes(attributes, skills, objects, forReplay);
        PopulateRhythmAttributes(attributes, skills);
        PopulateFlashlightAttributes(attributes, skills);
        PopulateReadingAttributes(attributes, skills);

        if (beatmap.Mods.Contains(typeof(ModRelax)))
        {
            attributes.AimDifficulty *= 0.9;
            attributes.TapDifficulty = 0;
            attributes.RhythmDifficulty = 0;
            attributes.FlashlightDifficulty *= 0.7;
            attributes.ReadingDifficulty *= 0.7;
        }

        if (beatmap.Mods.Contains(typeof(ModAutopilot)))
        {
            attributes.AimDifficulty = 0;
            attributes.FlashlightDifficulty *= 0.3;
            attributes.ReadingDifficulty *= 0.4;
        }

        double baseAimPerformance = DroidAim.DifficultyToPerformance(attributes.AimDifficulty);
        double baseTapPerformance = StrainSkill<DroidDifficultyHitObject>.DifficultyToPerformance(attributes.TapDifficulty);
        double baseFlashlightPerformance = DroidFlashlight.DifficultyToPerformance(attributes.FlashlightDifficulty);
        double baseReadingPerformance = DroidReading.DifficultyToPerformance(attributes.ReadingDifficulty);
        double basePerformance = System.Math.Pow(
            System.Math.Pow(baseAimPerformance, 1.1) +
            System.Math.Pow(baseTapPerformance, 1.1) +
            System.Math.Pow(baseFlashlightPerformance, 1.1) +
            System.Math.Pow(baseReadingPerformance, 1.1), 1 / 1.1);

        attributes.StarRating = basePerformance > 1e-5
            ? 0.027 * (System.Math.Cbrt(100000 / System.Math.Pow(2, 1 / 1.1) * basePerformance) + 4)
            : 0;

        return attributes;
    }

    protected override Skill<DroidDifficultyHitObject>[] CreateSkills(DroidPlayableBeatmap beatmap, bool forReplay)
    {
        IReadOnlyCollection<Mod> mods = beatmap.Mods.Values;
        List<Skill<DroidDifficultyHitObject>> skills = [];

        if (!beatmap.Mods.Contains(typeof(ModAutopilot)))
        {
            skills.Add(new DroidAim(mods, true));
            skills.Add(new DroidAim(mods, false));
        }

        if (!beatmap.Mods.Contains(typeof(ModRelax)))
        {
            skills.Add(new DroidRhythm(mods));
            skills.Add(new DroidTap(mods, true));
            skills.Add(new DroidTap(mods, true, 50));

            if (forReplay)
            {
                skills.Add(new DroidTap(mods, false));
            }
        }

        if (beatmap.Mods.Contains(typeof(ModFlashlight)))
        {
            skills.Add(new DroidFlashlight(mods, true));

            if (forReplay)
            {
                skills.Add(new DroidFlashlight(mods, false));
            }
        }

        skills.Add(new DroidReading(mods, beatmap.SpeedMultiplier, beatmap.HitObjects.Objects));
        return [.. skills];
    }

    protected override DroidDifficultyHitObject[] CreateDifficultyHitObjects(DroidPlayableBeatmap beatmap)
    {
        if (beatmap.HitObjects.Objects.Count == 0)
        {
            return [];
        }

        double clockRate = beatmap.SpeedMultiplier;
        var objects = beatmap.HitObjects.Objects;
        var result = new DroidDifficultyHitObject[objects.Count];

        for (int i = 0; i < objects.Count; ++i)
        {
            result[i] = new DroidDifficultyHitObject(
                objects[i],
                i > 0 ? objects[i - 1] : null,
                clockRate,
                result,
                i - 1);
            result[i].ComputeProperties(clockRate);
        }

        return result;
    }

    protected override DroidPlayableBeatmap CreatePlayableBeatmap(Beatmap beatmap, IEnumerable<Mod>? mods) =>
        beatmap.CreateDroidPlayableBeatmap(mods);

    private void PopulateAimAttributes(DroidDifficultyAttributes attributes, IEnumerable<Skill<DroidDifficultyHitObject>> skills, bool forReplay)
    {
        DroidAim? aim = FindSkill<DroidAim>(skills, skill => skill.WithSliders);
        if (aim is null)
        {
            return;
        }

        attributes.AimDifficulty = CalculateRating(aim);
        attributes.AimDifficultStrainCount = aim.CountTopWeightedStrains();
        attributes.AimDifficultSliderCount = aim.CountDifficultSliders();

        if (attributes.AimDifficulty > 0)
        {
            DroidAim aimNoSlider = FindSkill<DroidAim>(skills, skill => !skill.WithSliders)!;
            attributes.AimSliderFactor = CalculateRating(aimNoSlider) / attributes.AimDifficulty;
        }
        else
        {
            attributes.AimSliderFactor = 1;
        }

        if (!forReplay)
        {
            return;
        }

        double velocitySum = aim.SliderVelocities.Sum(s => s.DifficultyRating);
        foreach (DifficultSlider slider in aim.SliderVelocities)
        {
            double difficultyRating = slider.DifficultyRating / velocitySum;
            if (difficultyRating > 0.02)
            {
                attributes.DifficultSliders.Add(slider with { DifficultyRating = difficultyRating });
            }
        }

        attributes.DifficultSliders.Sort((a, b) => b.DifficultyRating.CompareTo(a.DifficultyRating));
        int limit = (int)System.Math.Ceiling(0.15 * attributes.SliderCount);
        if (attributes.DifficultSliders.Count > limit)
        {
            attributes.DifficultSliders.RemoveRange(limit, attributes.DifficultSliders.Count - limit);
        }
    }

    private void PopulateTapAttributes(
        DroidDifficultyAttributes attributes,
        IEnumerable<Skill<DroidDifficultyHitObject>> skills,
        IReadOnlyList<DroidDifficultyHitObject> objects,
        bool forReplay)
    {
        DroidTap? tap = FindSkill<DroidTap>(skills, skill => skill.ConsiderCheesability);
        DroidTap? tapVibro = FindSkill<DroidTap>(skills, skill => skill.ConsiderCheesability && skill.StrainTimeCap.HasValue);
        if (tap is null || tapVibro is null)
        {
            return;
        }

        attributes.TapDifficulty = CalculateRating(tap);
        attributes.TapDifficultStrainCount = tap.CountTopWeightedStrains();
        attributes.SpeedNoteCount = tap.RelevantNoteCount();
        attributes.AverageSpeedDeltaTime = tap.RelevantDeltaTime();

        if (attributes.TapDifficulty > 0)
        {
            attributes.VibroFactor = CalculateRating(tapVibro) / attributes.TapDifficulty;
        }

        if (!forReplay)
        {
            return;
        }

        DroidTap? tapNoCheese = FindSkill<DroidTap>(skills, skill => !skill.ConsiderCheesability);
        if (tapNoCheese is null)
        {
            return;
        }

        bool inSpeedSection = false;
        int firstSpeedObjectIndex = 0;
        double clockRate = attributes.ClockRate;

        for (int i = 2; i < objects.Count; ++i)
        {
            DroidDifficultyHitObject current = objects[i];
            DroidDifficultyHitObject prev = objects[i - 1];
            DroidDifficultyHitObject prevPrev = objects[i - 2];
            double currentStrain = tapNoCheese.ObjectStrains[i];

            if (!inSpeedSection && currentStrain >= ThreeFingerStrainThreshold)
            {
                inSpeedSection = true;
                firstSpeedObjectIndex = i;
                continue;
            }

            double currentDelta = (current.StartTime - prev.StartTime) / clockRate;
            double prevDelta = (prev.StartTime - prevPrev.StartTime) / clockRate;
            double deltaRatio = System.Math.Min(prevDelta, currentDelta) / System.Math.Max(prevDelta, currentDelta);

            if (inSpeedSection &&
                (currentStrain < ThreeFingerStrainThreshold ||
                 (prevDelta < currentDelta && deltaRatio <= 0.5) ||
                 i == objects.Count - 1))
            {
                int lastSpeedObjectIndex = i - (i == objects.Count - 1 ? 0 : 1);
                inSpeedSection = false;

                if (i - firstSpeedObjectIndex < MinimumSectionObjectCount)
                {
                    continue;
                }

                attributes.PossibleThreeFingeredSections.Add(new HighStrainSection(
                    firstSpeedObjectIndex,
                    lastSpeedObjectIndex,
                    CalculateThreeFingerSummedStrain(tapNoCheese.ObjectStrains.GetRange(firstSpeedObjectIndex, lastSpeedObjectIndex - firstSpeedObjectIndex))));
            }
        }
    }

    private void PopulateRhythmAttributes(DroidDifficultyAttributes attributes, IEnumerable<Skill<DroidDifficultyHitObject>> skills)
    {
        DroidRhythm? rhythm = FindSkill<DroidRhythm>(skills);
        if (rhythm is not null)
        {
            attributes.RhythmDifficulty = CalculateRating(rhythm);
        }
    }

    private void PopulateFlashlightAttributes(DroidDifficultyAttributes attributes, IEnumerable<Skill<DroidDifficultyHitObject>> skills)
    {
        DroidFlashlight? flashlight = FindSkill<DroidFlashlight>(skills, skill => skill.WithSliders);
        if (flashlight is null)
        {
            return;
        }

        DroidFlashlight? flashlightNoSlider = FindSkill<DroidFlashlight>(skills, skill => !skill.WithSliders);
        attributes.FlashlightDifficulty = CalculateRating(flashlight);
        attributes.FlashlightDifficultStrainCount = flashlight.CountTopWeightedStrains();
        attributes.FlashlightSliderFactor =
            flashlightNoSlider is not null && attributes.FlashlightDifficulty > 0
                ? CalculateRating(flashlightNoSlider) / attributes.FlashlightDifficulty
                : 1;
    }

    private void PopulateReadingAttributes(DroidDifficultyAttributes attributes, IEnumerable<Skill<DroidDifficultyHitObject>> skills)
    {
        DroidReading? reading = FindSkill<DroidReading>(skills);
        if (reading is null)
        {
            return;
        }

        attributes.ReadingDifficulty = CalculateRating(reading);
        attributes.ReadingDifficultNoteCount = reading.CountTopWeightedNotes();

        double greatWindow = attributes.Mods.Any(static mod => mod is ModPrecise)
            ? new ReferencePreciseDroidHitWindow(attributes.OverallDifficulty).GreatWindow
            : new ReferenceDroidHitWindow(attributes.OverallDifficulty).GreatWindow;

        double standardOverallDifficulty = ReferenceUnadjustedStandardHitWindow.HitWindow300ToOverallDifficulty(greatWindow);
        double ratingMultiplier = 0.75 + System.Math.Pow(System.Math.Max(0d, standardOverallDifficulty), 2.2) / 800;
        attributes.ReadingDifficulty *= System.Math.Sqrt(ratingMultiplier);
    }

    private static double CalculateThreeFingerSummedStrain(IReadOnlyList<double> strains) =>
        System.Math.Pow(strains.Sum(strain => strain / ThreeFingerStrainThreshold), 0.75);

    private static double CalculateRating(Skill<DroidDifficultyHitObject> skill) =>
        System.Math.Sqrt(skill.DifficultyValue()) * DifficultyMultiplier;
}
