#pragma warning disable CA1716

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

public abstract class Mod
{
    public virtual float ScoreMultiplier => 1f;

    public virtual bool RequiresConfiguration => false;

    public virtual bool IsRelevant => true;
}

public abstract class ReferenceMod : Mod
{
}
