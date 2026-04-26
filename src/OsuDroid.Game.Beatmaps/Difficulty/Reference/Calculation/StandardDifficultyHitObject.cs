using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation;

internal sealed class StandardDifficultyHitObject(
    HitObject obj,
    HitObject lastObj,
    double clockRate,
    StandardDifficultyHitObject[] difficultyHitObjects,
    int index
) : DifficultyHitObject(obj, lastObj, clockRate, difficultyHitObjects, index)
{
    protected override GameMode Mode => GameMode.Standard;

    public override double SmallCircleBonus =>
        System.Math.Max(1d, 1 + (30 - Obj.DifficultyRadius) / 40d);
}
