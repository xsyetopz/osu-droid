namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;

internal sealed class TimedDifficultyAttributes<TAttributes>(double time, TAttributes attributes) : IComparable<TimedDifficultyAttributes<TAttributes>>
    where TAttributes : DifficultyAttributes
{
    public double Time { get; } = time;

    public TAttributes Attributes { get; } = attributes;

    public int CompareTo(TimedDifficultyAttributes<TAttributes>? other) => other is null ? 1 : Time.CompareTo(other.Time);
}
