using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation;

internal abstract class DifficultyHitObject
{
    protected DifficultyHitObject(
        HitObject obj,
        HitObject? lastObj,
        double clockRate,
        DifficultyHitObject[] difficultyHitObjects,
        int index
    )
    {
        Obj = obj;
        this.lastObj = lastObj;
        this.difficultyHitObjects = difficultyHitObjects;
        Index = index;
        DeltaTime = lastObj is null ? 0d : (obj.StartTime - lastObj.StartTime) / clockRate;
        StrainTime = lastObj is null ? 0d : System.Math.Max(DeltaTime, MinDeltaTime);
        StartTime = obj.StartTime / clockRate;
        EndTime = obj.EndTime / clockRate;
        FullGreatWindow =
            (((obj is Slider slider ? slider.Head : obj).HitWindow?.GreatWindow) ?? 1200d)
            * 2
            / clockRate;
    }

    public const float NormalizedRadius = 50f;
    public const float NormalizedDiameter = NormalizedRadius * 2f;
    public const int MinDeltaTime = 25;

    public HitObject Obj { get; }

    public int Index { get; }

    public double LazyJumpDistance { get; protected set; }

    public double MinimumJumpDistance { get; protected set; }

    public double MinimumJumpTime { get; protected set; } = MinDeltaTime;

    public double TravelDistance { get; protected set; }

    public double TravelTime { get; protected set; } = MinDeltaTime;

    public ReferenceVector2? LazyEndPosition { get; protected set; }

    public double LazyTravelDistance { get; protected set; }

    public double LazyTravelTime { get; protected set; }

    public double? Angle { get; protected set; }

    public double DeltaTime { get; }

    public double StrainTime { get; }

    public double StartTime { get; }

    public double EndTime { get; }

    public double FullGreatWindow { get; }

    public abstract double SmallCircleBonus { get; }

    protected abstract GameMode Mode { get; }

    protected virtual float MaximumSliderRadius => NormalizedRadius * 2.4f;

    private readonly float assumedSliderRadius = NormalizedRadius * 1.8f;
    private readonly HitObject? lastObj;
    protected readonly DifficultyHitObject[] difficultyHitObjects;

    public virtual void ComputeProperties(double clockRate)
    {
        ComputeSliderCursorPosition();
        SetDistances(clockRate);
    }

    public virtual DifficultyHitObject? Previous(int backwardsIndex)
    {
        int prevIndex = Index - (backwardsIndex + 1);
        return prevIndex >= 0 ? difficultyHitObjects[prevIndex] : null;
    }

    public virtual DifficultyHitObject? Next(int forwardsIndex)
    {
        int nextIndex = Index + forwardsIndex + 1;
        return nextIndex < difficultyHitObjects.Length ? difficultyHitObjects[nextIndex] : null;
    }

    public virtual double OpacityAt(double time, IEnumerable<Mod> mods)
    {
        if (time > Obj.StartTime)
        {
            return 0d;
        }

        double fadeInStartTime = Obj.StartTime - Obj.TimePreempt;
        double fadeInDuration = Obj.TimeFadeIn;
        double nonHiddenOpacity = System.Math.Clamp(
            (time - fadeInStartTime) / fadeInDuration,
            0d,
            1d
        );

        if (mods.Any(static m => m is ModHidden))
        {
            double fadeOutStartTime = fadeInStartTime + fadeInDuration;
            double fadeOutDuration = Obj.TimePreempt * ModHidden.FadeOutDurationMultiplier;
            return System.Math.Min(
                nonHiddenOpacity,
                1 - System.Math.Clamp((time - fadeOutStartTime) / fadeOutDuration, 0d, 1d)
            );
        }

        return nonHiddenOpacity;
    }

    public double GetDoubletapness(DifficultyHitObject? nextObj)
    {
        if (nextObj is null)
        {
            return 0d;
        }

        double currentDeltaTime = System.Math.Max(1d, DeltaTime);
        double nextDeltaTime = System.Math.Max(1d, nextObj.DeltaTime);
        double deltaDifference = System.Math.Abs(nextDeltaTime - currentDeltaTime);
        double speedRatio = currentDeltaTime / System.Math.Max(currentDeltaTime, deltaDifference);
        double windowRatio = System.Math.Pow(
            System.Math.Min(1d, currentDeltaTime / FullGreatWindow),
            2
        );
        return 1 - System.Math.Pow(speedRatio, 1 - windowRatio);
    }

    private void SetDistances(double clockRate)
    {
        if (Obj is Slider slider)
        {
            TravelDistance =
                LazyTravelDistance
                * (
                    Mode switch
                    {
                        GameMode.Droid => System.Math.Pow(1 + slider.RepeatCount / 4d, 1d / 4d),
                        GameMode.Standard => System.Math.Pow(
                            1 + slider.RepeatCount / 2.5d,
                            1d / 2.5d
                        ),
                        _ => System.Math.Pow(1 + slider.RepeatCount / 2.5d, 1d / 2.5d),
                    }
                );

            TravelTime = System.Math.Max(LazyTravelTime / clockRate, MinDeltaTime);
        }

        if (lastObj is null || Obj is Spinner || lastObj is Spinner)
        {
            return;
        }

        DifficultyHitObject? lastDifficultyObject = Previous(0);
        DifficultyHitObject? lastLastDifficultyObject = Previous(1);

        float scalingFactor = (float)(NormalizedRadius / Obj.DifficultyRadius);
        ReferenceVector2 lastCursorPosition = lastDifficultyObject is null
            ? lastObj.DifficultyStackedPosition
            : GetEndCursorPosition(lastDifficultyObject);

        LazyJumpDistance = (
            Obj.DifficultyStackedPosition * scalingFactor - lastCursorPosition * scalingFactor
        ).Length;
        MinimumJumpTime = StrainTime;
        MinimumJumpDistance = LazyJumpDistance;

        if (lastObj is Slider lastSlider && lastDifficultyObject is not null)
        {
            double lastTravelTime = System.Math.Max(
                lastDifficultyObject.LazyTravelTime / clockRate,
                MinDeltaTime
            );
            MinimumJumpTime = System.Math.Max(StrainTime - lastTravelTime, MinDeltaTime);
            float tailJumpDistance =
                (lastSlider.Tail.DifficultyStackedPosition - Obj.DifficultyStackedPosition).Length
                * scalingFactor;

            MinimumJumpDistance = System.Math.Max(
                0d,
                System.Math.Min(
                    LazyJumpDistance - (MaximumSliderRadius - assumedSliderRadius),
                    tailJumpDistance - MaximumSliderRadius
                )
            );
        }

        if (lastLastDifficultyObject is null || lastLastDifficultyObject.Obj is Spinner)
        {
            return;
        }

        ReferenceVector2 lastLastCursorPosition = GetEndCursorPosition(lastLastDifficultyObject);
        ReferenceVector2 v1 = lastLastCursorPosition - lastObj.DifficultyStackedPosition;
        ReferenceVector2 v2 = Obj.DifficultyStackedPosition - lastCursorPosition;
        float dot = v1.Dot(v2);
        float det = v1.X * v2.Y - v1.Y * v2.X;
        Angle = System.Math.Abs(System.Math.Atan2(det, dot));
    }

    private void ComputeSliderCursorPosition()
    {
        if (Obj is not Slider slider || LazyEndPosition is not null)
        {
            return;
        }

        double trackingEndTime = slider.EndTime;
        IReadOnlyList<HitObject> nestedObjects = slider.NestedHitObjects;

        if (Mode == GameMode.Standard)
        {
            trackingEndTime = System.Math.Max(
                slider.EndTime - Slider.DroidLastTickOffset,
                slider.StartTime + slider.Duration / 2
            );
            SliderTick? lastRealTick = null;

            for (int i = nestedObjects.Count - 2; i >= 1; --i)
            {
                HitObject current = nestedObjects[i];
                if (current is SliderTick tick)
                {
                    lastRealTick = tick;
                    break;
                }

                if (current is SliderRepeat)
                {
                    break;
                }
            }

            if (lastRealTick is not null && lastRealTick.StartTime > trackingEndTime)
            {
                trackingEndTime = lastRealTick.StartTime;
                var reordered = nestedObjects.ToList();
                reordered.Remove(lastRealTick);
                reordered.Add(lastRealTick);
                nestedObjects = reordered;
            }
        }

        if (Mode == GameMode.Droid)
        {
            LazyEndPosition = slider.DifficultyStackedPosition;
            if (Precision.AlmostEquals(slider.StartTime, slider.EndTime))
            {
                return;
            }
        }

        LazyTravelTime = trackingEndTime - slider.StartTime;
        double endTimeMin = LazyTravelTime / slider.SpanDuration;
        endTimeMin = endTimeMin % 2 >= 1 ? 1 - endTimeMin % 1 : endTimeMin % 1;

        LazyEndPosition = slider.DifficultyStackedPosition + slider.Path.PositionAt(endTimeMin);

        ReferenceVector2 currentCursorPosition = slider.DifficultyStackedPosition;
        float scalingFactor = (float)(NormalizedRadius / slider.DifficultyRadius);

        for (int i = 1; i < nestedObjects.Count; ++i)
        {
            HitObject movementObject = nestedObjects[i];
            ReferenceVector2 movement =
                movementObject.DifficultyStackedPosition - currentCursorPosition;
            double movementLength = scalingFactor * movement.Length;
            double requiredMovement = assumedSliderRadius;

            if (i == nestedObjects.Count - 1)
            {
                ReferenceVector2 lazyMovement = LazyEndPosition.Value - currentCursorPosition;
                if (lazyMovement.Length < movement.Length)
                {
                    movement = lazyMovement;
                }

                movementLength = scalingFactor * movement.Length;
            }
            else if (movementObject is SliderRepeat)
            {
                requiredMovement = NormalizedRadius;
            }

            if (movementLength > requiredMovement)
            {
                currentCursorPosition +=
                    movement * ((movementLength - requiredMovement) / movementLength);
                movementLength *= (movementLength - requiredMovement) / movementLength;
                LazyTravelDistance += movementLength;
            }

            if (i == nestedObjects.Count - 1)
            {
                LazyEndPosition = currentCursorPosition;
            }
        }
    }

    private static ReferenceVector2 GetEndCursorPosition(DifficultyHitObject obj) =>
        obj.LazyEndPosition ?? obj.Obj.DifficultyStackedPosition;
}
