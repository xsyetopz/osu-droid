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

public sealed record MenuNowPlayingState(string? ArtistTitle = null, bool IsPlaying = false);

public sealed record GameServices(
    DroidDatabase Database,
    string CorePath,
    string BuildType,
    string DisplayVersion = "1.0",
    IMenuMusicController? MusicController = null,
    MenuNowPlayingState? NowPlaying = null);
