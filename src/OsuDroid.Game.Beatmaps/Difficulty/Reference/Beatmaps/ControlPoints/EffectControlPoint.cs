namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal sealed class EffectControlPoint(double time, bool isKiai) : ControlPoint(time)
{
    public bool IsKiai { get; } = isKiai;

    public override bool IsRedundant(ControlPoint existing) =>
        existing is EffectControlPoint effectControlPoint && IsKiai == effectControlPoint.IsKiai;
}
