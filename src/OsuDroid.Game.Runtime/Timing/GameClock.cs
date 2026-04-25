namespace OsuDroid.Game.Runtime.Timing;

public interface IGameClock
{
    TimeSpan Elapsed { get; }
}

public sealed class ManualGameClock : IGameClock
{
    public TimeSpan Elapsed { get; private set; }

    public void Advance(TimeSpan elapsed) => Elapsed = elapsed;
}
