using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;

internal sealed class BeatmapControlPoints
{
    public static readonly int[] PredefinedDivisors = [1, 2, 3, 4, 6, 8, 12, 16];

    public TimingControlPointManager Timing { get; } = new();

    public DifficultyControlPointManager Difficulty { get; } = new();

    public EffectControlPointManager Effect { get; } = new();

    public SampleControlPointManager Sample { get; } = new();

    public int GetClosestBeatDivisor(double time)
    {
        TimingControlPoint timingPoint = Timing.ControlPointAt(time);
        int closestDivisor = 0;
        double closestTime = double.MaxValue;

        foreach (int divisor in PredefinedDivisors)
        {
            double distanceFromSnap = System.Math.Abs(
                time - GetClosestSnappedTime(timingPoint, time, divisor)
            );
            if (Precision.DefinitelyBigger(closestTime, distanceFromSnap))
            {
                closestDivisor = divisor;
                closestTime = distanceFromSnap;
            }
        }

        return closestDivisor;
    }

    private static double GetClosestSnappedTime(
        TimingControlPoint timingPoint,
        double time,
        int beatDivisor
    )
    {
        double beatLength = timingPoint.MillisecondsPerBeat / beatDivisor;
        int beats = (int)
            System.Math.Round(
                (System.Math.Max(time, 0) - timingPoint.Time) / beatLength,
                MidpointRounding.AwayFromZero
            );
        double snappedTime = timingPoint.Time + beats * beatLength;
        return snappedTime >= 0 ? snappedTime : snappedTime + beatLength;
    }
}
