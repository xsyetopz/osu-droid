using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime.Paths;

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
    DroidGamePathLayout Paths,
    string BuildType,
    string DisplayVersion = "1.0",
    IMenuMusicController? MusicController = null,
    MenuNowPlayingState? NowPlaying = null)
{
    public string CorePath => Paths.CoreRoot;
}
