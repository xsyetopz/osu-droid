namespace OsuDroid.Game.UI.Scrolling;

public sealed class DroidScrollableRegion(KineticScrollAxis axis)
{
    private readonly KineticScrollState _scroll = new(axis);

    public float Offset { get; private set; }

    public float Velocity => _scroll.Velocity;

    public bool Begin(UiPoint point, double timestampSeconds, float maxOffset)
    {
        if (maxOffset <= 0f)
        {
            return false;
        }

        _scroll.Begin(point, timestampSeconds);
        return true;
    }

    public bool Drag(UiPoint point, double timestampSeconds, float maxOffset)
    {
        bool changed = _scroll.Drag(point, timestampSeconds, () => Offset, value => Offset = value, 0f, maxOffset);
        Offset = Math.Clamp(Offset, 0f, maxOffset);
        return changed;
    }

    public void End(UiPoint point, double timestampSeconds, float maxOffset)
    {
        _scroll.End(point, timestampSeconds, () => Offset, value => Offset = value, 0f, maxOffset);
        Offset = Math.Clamp(Offset, 0f, maxOffset);
    }

    public bool Update(float elapsedSeconds, float maxOffset)
    {
        bool changed = _scroll.Update(elapsedSeconds, () => Offset, value => Offset = value, 0f, maxOffset);
        Offset = Math.Clamp(Offset, 0f, maxOffset);
        return changed;
    }

    public void ScrollBy(float delta, float maxOffset)
    {
        _scroll.Stop();
        Offset = Math.Clamp(Offset + delta, 0f, maxOffset);
    }

    public void Clamp(float maxOffset) => Offset = Math.Clamp(Offset, 0f, maxOffset);

    public void Stop() => _scroll.Stop();
}
