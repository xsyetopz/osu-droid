namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal sealed class TimingControlPointManager()
    : ControlPointManager<TimingControlPoint>(new TimingControlPoint(0, 1000, 4))
{
    public override TimingControlPoint ControlPointAt(double time) =>
        BinarySearchWithFallback(
            time,
            ControlPoints.Count > 0 ? ControlPoints[0] : DefaultControlPoint
        );
}
