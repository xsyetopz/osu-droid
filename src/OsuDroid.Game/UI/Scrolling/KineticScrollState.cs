namespace OsuDroid.Game.UI.Scrolling;

public enum KineticScrollAxis
{
    Horizontal,
    Vertical,
}

public sealed class KineticScrollState(KineticScrollAxis axis)
{
    private const float DecelerationPerFrame = 0.98f;
    private const float FrameRate = 60f;
    private const float MinimumTravel = 20f;
    private const float MaxVelocity = 3000f;
    private const float StopVelocity = 1f;
    private const float MaxFlingSampleSeconds = 0.3f;

    private UiPoint _startPoint;
    private UiPoint _lastPoint;
    private double _lastTimestampSeconds;
    private UiPoint _flingSamplePoint;
    private double _flingSampleTimestampSeconds;
    private bool _isTracking;
    private bool _isDragging;

    public bool IsDragging => _isDragging;

    public void Begin(UiPoint point, double timestampSeconds)
    {
        _startPoint = point;
        _lastPoint = point;
        _lastTimestampSeconds = timestampSeconds;
        _flingSamplePoint = point;
        _flingSampleTimestampSeconds = timestampSeconds;
        _isTracking = true;
        _isDragging = false;
        Velocity = 0f;
    }

    public float Velocity { get; private set; }

    public bool Drag(UiPoint point, double timestampSeconds, Func<float> getOffset, Action<float> setOffset, float minOffset, float maxOffset)
    {
        if (!_isTracking)
        {
            return false;
        }

        float totalTravel = AxisDelta(_startPoint, point);
        if (!_isDragging && MathF.Abs(totalTravel) < MinimumTravel)
        {
            _lastPoint = point;
            _lastTimestampSeconds = timestampSeconds;
            return false;
        }

        if (!_isDragging)
        {
            _flingSamplePoint = point;
            _flingSampleTimestampSeconds = timestampSeconds;
        }

        float delta = AxisDelta(_lastPoint, point);
        float elapsed = Math.Max(1f / 120f, (float)(timestampSeconds - _lastTimestampSeconds));
        float flingElapsed = (float)(timestampSeconds - _flingSampleTimestampSeconds);
        Velocity = Math.Clamp(
            flingElapsed is > 0f and <= MaxFlingSampleSeconds
                ? AxisDelta(_flingSamplePoint, point) / flingElapsed
                : delta / elapsed,
            -MaxVelocity,
            MaxVelocity);
        setOffset(Math.Clamp(getOffset() + delta, minOffset, maxOffset));
        _lastPoint = point;
        _lastTimestampSeconds = timestampSeconds;
        _isDragging = true;
        return true;
    }

    public bool UpdateLinear(
        float elapsedSeconds,
        float decelerationPerSecond,
        Func<float> getOffset,
        Action<float> setOffset,
        float minOffset,
        float maxOffset)
    {
        if (_isTracking || elapsedSeconds <= 0f)
        {
            return false;
        }

        if (MathF.Abs(Velocity) <= decelerationPerSecond * elapsedSeconds)
        {
            Velocity = 0f;
            return false;
        }

        float offset = getOffset();
        float next = Math.Clamp(offset + Velocity * elapsedSeconds, minOffset, maxOffset);
        setOffset(next);

        if (next <= minOffset || next >= maxOffset)
        {
            Velocity = 0f;
            return next != offset;
        }

        Velocity -= decelerationPerSecond * elapsedSeconds * MathF.Sign(Velocity);
        return true;
    }

    public void End()
    {
        _isTracking = false;
        _isDragging = false;
    }

    public void End(UiPoint point, double timestampSeconds, Func<float> getOffset, Action<float> setOffset, float minOffset, float maxOffset)
    {
        Drag(point, timestampSeconds, getOffset, setOffset, minOffset, maxOffset);
        End();
    }

    public bool Update(float elapsedSeconds, Func<float> getOffset, Action<float> setOffset, float minOffset, float maxOffset)
    {
        if (_isTracking || elapsedSeconds <= 0f || MathF.Abs(Velocity) <= StopVelocity)
        {
            return false;
        }

        float offset = getOffset();
        float next = Math.Clamp(offset + Velocity * elapsedSeconds, minOffset, maxOffset);
        setOffset(next);

        if (next <= minOffset || next >= maxOffset)
        {
            Velocity = 0f;
            return next != offset;
        }

        Velocity *= MathF.Pow(DecelerationPerFrame, elapsedSeconds * FrameRate);
        if (MathF.Abs(Velocity) <= StopVelocity)
        {
            Velocity = 0f;
        }

        return true;
    }

    public void Stop()
    {
        _isTracking = false;
        _isDragging = false;
        Velocity = 0f;
    }

    private float AxisDelta(UiPoint previous, UiPoint current) => axis == KineticScrollAxis.Horizontal
        ? previous.X - current.X
        : previous.Y - current.Y;
}
