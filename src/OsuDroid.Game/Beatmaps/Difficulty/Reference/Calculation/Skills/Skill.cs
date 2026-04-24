using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal abstract class Skill<TObject>(IEnumerable<Mod> mods)
    where TObject : DifficultyHitObject
{
    protected IEnumerable<Mod> Mods { get; } = mods;

    public abstract void Process(TObject current);

    public abstract double DifficultyValue();
}
