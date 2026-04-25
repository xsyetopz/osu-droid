namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal sealed class TimingControlPoint(double time, double msPerBeat, int timeSignature) : ControlPoint(time)
{
    public double MillisecondsPerBeat { get; } = msPerBeat;

    public int TimeSignature { get; } = timeSignature;

    public double BeatsPerMinute => 60000d / MillisecondsPerBeat;

    public override bool IsRedundant(ControlPoint existing) => false;
}
