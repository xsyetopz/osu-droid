namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal sealed class SampleControlPointManager()
    : ControlPointManager<SampleControlPoint>(new SampleControlPoint(0, SampleBank.Normal, 100, 0))
{
    public override SampleControlPoint ControlPointAt(double time) =>
        BinarySearchWithFallback(time, ControlPoints.Count > 0 ? ControlPoints[0] : DefaultControlPoint);
}
