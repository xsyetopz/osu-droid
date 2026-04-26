using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;

internal abstract class HitObject(double startTime, ReferenceVector2 position)
{
    public const double PreemptMax = ReferenceDifficultyTiming.PreemptMax;
    public const double PreemptMid = ReferenceDifficultyTiming.PreemptMid;
    public const double PreemptMin = ReferenceDifficultyTiming.PreemptMin;
    public const double ObjectRadius = 64.0;

    public double StartTime { get; } = startTime;

    public virtual double EndTime => StartTime;

    public virtual double Duration => EndTime - StartTime;

    public ReferenceVector2 Position { get; set; } = position;

    public virtual ReferenceVector2 EndPosition => Position;

    public double TimePreempt { get; set; } = 600.0;

    public double TimeFadeIn { get; set; } = 400.0;

    public ReferenceHitWindow? HitWindow { get; set; }

    public float DifficultyScale { get; set; } = 1f;

    public float StackOffsetMultiplier { get; set; }

    public int DifficultyStackHeight { get; set; }

    public double DifficultyRadius => ObjectRadius * DifficultyScale;

    public ReferenceVector2 DifficultyStackOffset =>
        new(
            DifficultyStackHeight * DifficultyScale * StackOffsetMultiplier,
            DifficultyStackHeight * DifficultyScale * StackOffsetMultiplier
        );

    public virtual ReferenceVector2 DifficultyStackedPosition => Position + DifficultyStackOffset;

    public virtual ReferenceVector2 DifficultyStackedEndPosition => DifficultyStackedPosition;

    public virtual void ApplyDefaults(
        ReferenceBeatmapDifficulty difficulty,
        BeatmapControlPoints controlPoints,
        GameMode mode
    )
    {
        double preempt = ReferenceBeatmapDifficulty.DifficultyRange(
            difficulty.ApproachRate,
            ReferenceDifficultyTiming.PreemptMax,
            ReferenceDifficultyTiming.PreemptMid,
            ReferenceDifficultyTiming.PreemptMin
        );

        TimePreempt = preempt;
        TimeFadeIn = System.Math.Min(400.0, preempt);
        DifficultyScale = CalculateScale(difficulty.DifficultyCircleSize, mode);
        StackOffsetMultiplier = mode == GameMode.Droid ? -4f : -6.4f;
        HitWindow =
            mode == GameMode.Standard
                ? new ReferenceStandardHitWindow(difficulty.OverallDifficulty)
                : new ReferenceDroidHitWindow(difficulty.OverallDifficulty);
    }

    private static float CalculateScale(float circleSize, GameMode mode)
    {
        const float droidStandardCircleSizeOffset = 6.855634f;
        const float brokenGamefieldRoundingAllowance = 1.00041f;
        float standardCircleSize =
            mode == GameMode.Droid ? circleSize - droidStandardCircleSizeOffset : circleSize;
        return System.Math.Max(
            1e-3f,
            (1 - 0.7f * (standardCircleSize - 5) / 5) / 2 * brokenGamefieldRoundingAllowance
        );
    }
}

internal sealed class HitCircle(double startTime, ReferenceVector2 position)
    : HitObject(startTime, position);

internal sealed class Spinner(double startTime, double endTime, ReferenceVector2 position)
    : HitObject(startTime, position)
{
    public override double EndTime { get; } = endTime;
}

internal class SliderHitObject(double startTime, ReferenceVector2 position)
    : HitObject(startTime, position);

internal sealed class SliderHead(double startTime, ReferenceVector2 position)
    : SliderHitObject(startTime, position);

internal class SliderEndCircle(double startTime, ReferenceVector2 position)
    : SliderHitObject(startTime, position);

internal sealed class SliderTail(double startTime, ReferenceVector2 position)
    : SliderEndCircle(startTime, position);

internal sealed class SliderRepeat(double startTime, ReferenceVector2 position, int spanIndex)
    : SliderEndCircle(startTime, position)
{
    public int SpanIndex { get; } = spanIndex;
}

internal sealed class SliderTick(double startTime, ReferenceVector2 position, int spanIndex)
    : SliderHitObject(startTime, position)
{
    public int SpanIndex { get; } = spanIndex;
}

internal sealed class Slider(
    double startTime,
    ReferenceVector2 position,
    int repeatCount,
    SliderPath path
) : HitObject(startTime, position)
{
    public const double DroidLastTickOffset = 36.0;

    private readonly List<HitObject> _nestedHitObjects = [];

    public SliderPath Path { get; } = path;

    public int RepeatCount { get; } = System.Math.Max(1, repeatCount);

    public int SpanCount => RepeatCount;

    public double Velocity { get; private set; } = 1.0;

    public double TickDistance { get; private set; } = 100.0;

    public double SpanDuration => Path.ExpectedDistance / Velocity;

    public override double EndTime => StartTime + RepeatCount * SpanDuration;

    public override ReferenceVector2 EndPosition =>
        Position + Path.PositionAt(RepeatCount % 2 == 0 ? 0.0 : 1.0);

    public override ReferenceVector2 DifficultyStackedEndPosition =>
        EndPosition + DifficultyStackOffset;

    public IReadOnlyList<HitObject> NestedHitObjects => _nestedHitObjects;

    public SliderHead Head { get; private set; } = new(startTime, position);

    public SliderTail Tail { get; private set; } = new(startTime, position);

    public override void ApplyDefaults(
        ReferenceBeatmapDifficulty difficulty,
        BeatmapControlPoints controlPoints,
        GameMode mode
    )
    {
        base.ApplyDefaults(difficulty, controlPoints, mode);
        var timingPoint = controlPoints.Timing.ControlPointAt(StartTime);
        var difficultyPoint = controlPoints.Difficulty.ControlPointAt(StartTime);
        double beatLength =
            timingPoint.MillisecondsPerBeat <= 0 ? 1.0 : timingPoint.MillisecondsPerBeat;
        double sliderVelocityAsBeatLength = -100.0 / difficultyPoint.SpeedMultiplier;
        double bpmMultiplier =
            sliderVelocityAsBeatLength < 0
                ? System.Math.Clamp(-sliderVelocityAsBeatLength, 10.0, 1000.0) / 100.0
                : 1.0;

        Velocity = 100.0 * difficulty.SliderMultiplier / (beatLength * bpmMultiplier);
        double scoringDistance = Velocity * beatLength;
        TickDistance = difficultyPoint.GenerateTicks
            ? scoringDistance / difficulty.SliderTickRate
            : double.PositiveInfinity;
        BuildNestedObjects(difficulty, controlPoints, mode);
    }

    private void BuildNestedObjects(
        ReferenceBeatmapDifficulty difficulty,
        BeatmapControlPoints controlPoints,
        GameMode mode
    )
    {
        _nestedHitObjects.Clear();
        Head = new SliderHead(StartTime, Position);
        Head.ApplyDefaults(difficulty, controlPoints, mode);
        _nestedHitObjects.Add(Head);

        double spanDuration = SpanDuration;
        double tickDistance = TickDistance;
        double minDistanceFromEnd = Velocity * 10.0;

        for (int span = 0; span < RepeatCount; span++)
        {
            bool reversed = span % 2 == 1;
            double spanStartTime = StartTime + span * spanDuration;

            if (tickDistance > 0)
            {
                for (
                    double distance = tickDistance;
                    distance < Path.ExpectedDistance - minDistanceFromEnd;
                    distance += tickDistance
                )
                {
                    double progress = distance / Path.ExpectedDistance;
                    double timeProgress = reversed ? 1.0 - progress : progress;
                    var tick = new SliderTick(
                        spanStartTime + timeProgress * spanDuration,
                        Position + Path.PositionAt(progress),
                        span
                    );
                    tick.ApplyDefaults(difficulty, controlPoints, mode);
                    _nestedHitObjects.Add(tick);
                }
            }

            if (span < RepeatCount - 1)
            {
                double repeatProgress = reversed ? 0.0 : 1.0;
                var repeat = new SliderRepeat(
                    StartTime + (span + 1) * spanDuration,
                    Position + Path.PositionAt(repeatProgress),
                    span
                );
                repeat.ApplyDefaults(difficulty, controlPoints, mode);
                _nestedHitObjects.Add(repeat);
            }
        }

        Tail = new SliderTail(EndTime, EndPosition);
        Tail.ApplyDefaults(difficulty, controlPoints, mode);
        _nestedHitObjects.Add(Tail);
        _nestedHitObjects.Sort((left, right) => left.StartTime.CompareTo(right.StartTime));
    }
}
