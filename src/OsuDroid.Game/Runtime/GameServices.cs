using OsuDroid.Game.Compatibility.Database;

namespace OsuDroid.Game.Runtime;

public interface IGameClock
{
    TimeSpan Elapsed { get; }
}

public sealed class ManualGameClock : IGameClock
{
    public TimeSpan Elapsed { get; private set; }

    public void Advance(TimeSpan elapsed) => Elapsed = elapsed;
}

public sealed record GameServices(DroidDatabase Database, string CorePath, string BuildType);
