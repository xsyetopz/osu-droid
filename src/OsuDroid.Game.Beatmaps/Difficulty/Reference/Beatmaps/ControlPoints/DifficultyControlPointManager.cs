namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal sealed class DifficultyControlPointManager()
    : ControlPointManager<DifficultyControlPoint>(new DifficultyControlPoint(0, 1, true))
{
    public override DifficultyControlPoint ControlPointAt(double time) =>
        BinarySearchWithFallback(time);
}
