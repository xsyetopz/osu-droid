namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal sealed class EffectControlPointManager()
    : ControlPointManager<EffectControlPoint>(new EffectControlPoint(0, false))
{
    public override EffectControlPoint ControlPointAt(double time) => BinarySearchWithFallback(time);
}
