#pragma warning disable IDE0290

using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation;

internal sealed class DroidDifficultyHitObject : DifficultyHitObject
{
    private readonly DroidDifficultyHitObject[] typedDifficultyHitObjects;

    public DroidDifficultyHitObject(
        HitObject obj,
        HitObject? lastObj,
        double clockRate,
        DroidDifficultyHitObject[] difficultyHitObjects,
        int index)
        : base(obj, lastObj, clockRate, difficultyHitObjects, index)
    {
        typedDifficultyHitObjects = difficultyHitObjects;
        TimePreempt = obj.TimePreempt / clockRate;
    }

    protected override GameMode Mode => GameMode.Droid;

    protected override float MaximumSliderRadius => NormalizedRadius * 2f;

    public override double SmallCircleBonus => System.Math.Max(1d, 1 + System.Math.Pow((70 - Obj.DifficultyRadius) / 50d, 2));

    public double RhythmMultiplier { get; set; } = 1d;

    public double TimePreempt { get; }

    public override DifficultyHitObject? Previous(int backwardsIndex) =>
        Index - backwardsIndex >= 0 ? typedDifficultyHitObjects[Index - backwardsIndex] : null;

    public override DifficultyHitObject? Next(int forwardsIndex)
    {
        int nextIndex = Index + forwardsIndex + 2;
        return nextIndex < typedDifficultyHitObjects.Length ? typedDifficultyHitObjects[nextIndex] : null;
    }

    public override double OpacityAt(double time, IEnumerable<Mod> mods)
    {
        return Obj is HitCircle && mods.Any(static mod => mod is ModTraceable) ? 0d : base.OpacityAt(time, mods);
    }

    public bool IsOverlapping(bool considerDistance)
    {
        if (Obj is Spinner)
        {
            return false;
        }

        var previous = Previous(0) as DroidDifficultyHitObject;
        if (previous is null || previous.Obj is Spinner || DeltaTime >= 5)
        {
            return false;
        }

        if (!considerDistance)
        {
            return true;
        }

        var position = Obj.DifficultyStackedPosition;
        float distanceSquared = previous.Obj.DifficultyStackedEndPosition.DistanceSquaredTo(position);
        if (previous.LazyEndPosition is { } lazyEndPosition)
        {
            distanceSquared = System.Math.Min(distanceSquared, lazyEndPosition.DistanceSquaredTo(position));
        }

        float threshold = (float)(2 * Obj.DifficultyRadius);
        return distanceSquared <= threshold * threshold;
    }
}
