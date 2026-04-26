namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;

internal sealed class Island(double epsilon) : IEquatable<Island>
{
    public int Delta { get; private set; } = int.MaxValue;

    public int DeltaCount { get; private set; }

    private readonly double deltaDifferenceEpsilon = epsilon;

    public Island(int delta, double epsilon)
        : this(epsilon)
    {
        AddDelta(delta);
    }

    public void AddDelta(int delta)
    {
        if (Delta == int.MaxValue)
        {
            Delta = System.Math.Max(delta, DifficultyHitObject.MinDeltaTime);
        }

        ++DeltaCount;
    }

    public bool IsSimilarPolarity(Island other) => DeltaCount % 2 == other.DeltaCount % 2;

    public bool Equals(Island? other) =>
        other is not null
        && System.Math.Abs(Delta - other.Delta) < deltaDifferenceEpsilon
        && DeltaCount == other.DeltaCount;

    public override bool Equals(object? obj) => obj is Island other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Delta, DeltaCount);
}
