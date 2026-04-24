namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal abstract class ControlPoint(double time)
{
    public double Time { get; } = time;

    public abstract bool IsRedundant(ControlPoint existing);
}
