namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;

internal sealed class HighStrainSection(int firstObjectIndex, int lastObjectIndex, double sumStrain)
{
    public int FirstObjectIndex { get; } = firstObjectIndex;

    public int LastObjectIndex { get; } = lastObjectIndex;

    public double SumStrain { get; } = sumStrain;
}
