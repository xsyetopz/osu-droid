#pragma warning disable CA1822

using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Calculators;

internal abstract class DifficultyCalculator<TBeatmap, TObject, TAttributes>
    where TBeatmap : PlayableBeatmap
    where TObject : DifficultyHitObject
    where TAttributes : DifficultyAttributes
{
    protected virtual HashSet<Type> DifficultyAdjustmentMods { get; } =
    [
        typeof(ModRelax),
        typeof(ModAutopilot),
        typeof(ModEasy),
        typeof(ModReallyEasy),
        typeof(ModMirror),
        typeof(ModHardRock),
        typeof(ModHidden),
        typeof(ModFlashlight),
        typeof(ModDifficultyAdjust),
        typeof(ModRateAdjust),
        typeof(ModTimeRamp),
        typeof(ModTraceable),
    ];

    public ISet<Mod> RetainDifficultyAdjustmentMods(IEnumerable<Mod>? mods)
    {
        return mods is null
            ? new HashSet<Mod>()
            : mods.Where(mod => DifficultyAdjustmentMods.Any(t => t.IsInstanceOfType(mod)))
                .ToHashSet();
    }

    public TAttributes Calculate(Beatmap beatmap, IEnumerable<Mod>? mods = null) =>
        Calculate(CreatePlayableBeatmap(beatmap, mods));

    public TAttributes Calculate(TBeatmap beatmap)
    {
        Skill<TObject>[] skills = CreateSkills(beatmap, false);
        TObject[] objects = CreateDifficultyHitObjects(beatmap);

        foreach (TObject obj in objects)
        {
            foreach (Skill<TObject> skill in skills)
            {
                skill.Process(obj);
            }
        }

        return CreateDifficultyAttributes(beatmap, skills, objects, false);
    }

    public TAttributes CalculateForReplay(Beatmap beatmap, IEnumerable<Mod>? mods = null) =>
        CalculateForReplay(CreatePlayableBeatmap(beatmap, mods));

    public TAttributes CalculateForReplay(TBeatmap beatmap)
    {
        Skill<TObject>[] skills = CreateSkills(beatmap, true);
        TObject[] objects = CreateDifficultyHitObjects(beatmap);

        foreach (TObject obj in objects)
        {
            foreach (Skill<TObject> skill in skills)
            {
                skill.Process(obj);
            }
        }

        return CreateDifficultyAttributes(beatmap, skills, objects, true);
    }

    protected TSkill? FindSkill<TSkill>(
        IEnumerable<Skill<TObject>> skills,
        Func<TSkill, bool>? predicate = null
    )
        where TSkill : Skill<TObject>
    {
        foreach (Skill<TObject> skill in skills)
        {
            if (skill is not TSkill casted)
            {
                continue;
            }

            if (predicate is null || predicate(casted))
            {
                return casted;
            }
        }

        return null;
    }

    protected abstract Skill<TObject>[] CreateSkills(TBeatmap beatmap, bool forReplay);

    protected abstract TObject[] CreateDifficultyHitObjects(TBeatmap beatmap);

    protected abstract TAttributes CreateDifficultyAttributes(
        PlayableBeatmap beatmap,
        Skill<TObject>[] skills,
        TObject[] objects,
        bool forReplay
    );

    protected abstract TBeatmap CreatePlayableBeatmap(Beatmap beatmap, IEnumerable<Mod>? mods);
}
