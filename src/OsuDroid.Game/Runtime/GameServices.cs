using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.UI;

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

public sealed record MenuNowPlayingState(
    string? ArtistTitle = null,
    bool IsPlaying = false,
    int PositionMilliseconds = 0,
    int LengthMilliseconds = 0,
    float Bpm = 0f,
    string? BeatmapSetDirectory = null,
    string? BeatmapFilename = null);

public sealed record GameServices(
    DroidDatabase Database,
    DroidGamePathLayout Paths,
    string BuildType,
    string DisplayVersion = "1.0",
    IMenuMusicController? MusicController = null,
    MenuNowPlayingState? NowPlaying = null,
    IBeatmapLibrary? BeatmapLibrary = null,
    IBeatmapImportService? BeatmapImportService = null,
    IBeatmapProcessingService? BeatmapProcessingService = null,
    IBeatmapDownloadService? BeatmapDownloadService = null,
    IBeatmapMirrorClient? BeatmapMirrorClient = null,
    ITextInputService? TextInputService = null,
    IBeatmapPreviewPlayer? BeatmapPreviewPlayer = null,
    IBeatmapDifficultyService? BeatmapDifficultyService = null,
    IGameSettingsStore? SettingsStore = null,
    IMenuSfxPlayer? MenuSfxPlayer = null,
    OnlineProfileSnapshot? OnlineProfile = null,
    bool ShowStartupScene = false)
{
    public string CorePath => Paths.CoreRoot;
}
